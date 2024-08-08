﻿using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.Connections;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShiraSubathonTracker.DAL;
using ShiraSubathonTracker.DAL.Entities.Minecraft;

namespace ShiraSubathonTracker.MinecraftScanner;

public class ServerPinger(
    ILogger<ServerPinger> logger,
    StreamBuffer streamBuffer,
    TrackerDatabaseContext trackerDatabaseContext)
{
    private const int TimeToAdd = 15;

    [Function("ServerPing")]
    public async Task RunAsync([TimerTrigger("*/15 * * * * *")] TimerInfo myTimer, FunctionContext context)
    {
        var servers = await trackerDatabaseContext.MinecraftServers
            .Include(x => x.MinecraftVersion)
            .Where(x => x.CurrentServer)
            .ToListAsync(context.CancellationToken);

        foreach (var server in servers)
        {
            await PingServer(server, context.CancellationToken);
        }
    }

    private async Task PingServer(MinecraftServer server, CancellationToken cancellationToken)
    {
        var splitAddress = server.IpAddress.Split(":");
        var host = splitAddress[0];
        var port = int.Parse(splitAddress[1]);

        var tpcClient = new TcpClient();
        var task = tpcClient.ConnectAsync(host, port, cancellationToken);

        var ping = await EstablishConnection(server, task, tpcClient, cancellationToken);
        streamBuffer.HandshakeSetupTime = ping;

        await using var stream = tpcClient.GetStream();
        await SetupHandshake(stream, host, port, server.MinecraftVersion.ServerProtocol);

        SendStatusRequest(stream);

        var response = await ReadStreamData(stream, cancellationToken);
        await StoreServerData(server, response, cancellationToken);

        streamBuffer.Clear();
    }

    private async Task<long> EstablishConnection(MinecraftServer server, ValueTask task, TcpClient tpcClient,
        CancellationToken cancellationToken)
    {
        long ping;

        try
        {
            logger.LogInformation("Attempting connection to {serverAddress}...", server.IpAddress);
            await AwaitConnection(task, out var connectionTime);
            ping = connectionTime;
        }
        catch (TimeoutException e)
        {
            server.ServerStatus = ServerStatus.ConnectionTimedOut;
            await trackerDatabaseContext.SaveChangesAsync(cancellationToken);

            logger.LogError(e, "Connection to {serverAddress} timed out!", server.IpAddress);
            throw;
        }

        if (tpcClient.Connected) return ping;
        server.ServerStatus = ServerStatus.Offline;
        await trackerDatabaseContext.SaveChangesAsync(cancellationToken);

        logger.LogError("Could not establish connection to {serverAddress}.", server.IpAddress);
        throw new ConnectionAbortedException();
    }

    private static Task AwaitConnection(ValueTask task, out long ping)
    {
        const long timeOut = 5000;
        var startingTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        while (!task.IsCompleted)
        {
            currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (currentTime - startingTime >= timeOut)
            {
                throw new TimeoutException();
            }
        }

        ping = currentTime - startingTime;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Send a "Handshake" packet.
    /// http://wiki.vg/Server_List_Ping#Ping_Process
    /// </summary>
    /// <param name="stream">Stream used to set up handshake.</param>
    /// <param name="serverHost">The server host to connect to.</param>
    /// <param name="serverPort">The server port used for the connection.</param>
    /// <param name="serverProtocol">The server protocol used for the connection.</param>
    private Task SetupHandshake(NetworkStream stream, string serverHost, int serverPort, int serverProtocol)
    {
        logger.LogInformation("Attempting handshake.");
        streamBuffer.WriteInt(serverProtocol);
        streamBuffer.WriteString(serverHost);
        streamBuffer.WriteShort((short)serverPort);
        streamBuffer.WriteInt(1);

        streamBuffer.FlushToStream(stream, 0);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Send a "Status Request" packet
    /// http://wiki.vg/Server_List_Ping#Ping_Process
    /// </summary>
    /// <param name="stream">Stream used to send the packet to.</param>
    private void SendStatusRequest(NetworkStream stream)
    {
        logger.LogInformation("Sending status request packet.");
        streamBuffer.FlushToStream(stream, 0);
    }

    private Task<ServerPingResponse> ReadStreamData(NetworkStream stream, CancellationToken cancellationToken)
    {
        logger.LogInformation("Reading stream to buffer.");
        streamBuffer.ReadStreamToBuffer(stream);

        try
        {
            var packet = streamBuffer.ReadInt();
            var jsonLength = streamBuffer.ReadInt();

            logger.LogInformation("Received packet 0x{packedId} with a length of {length}", packet.ToString("X2"),
                streamBuffer.BufferLength);

            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true
            };

            var json = streamBuffer.ReadString(jsonLength);
            var pingResponse = JsonSerializer.Deserialize<ServerPingResponse>(json, options);
            return Task.FromResult(pingResponse!);
        }
        catch (IOException e)
        {
            logger.LogError(e, "Unable to read packet length from the server.");
            throw;
        }
    }

    private async Task StoreServerData(MinecraftServer server, ServerPingResponse serverPingResponse,
        CancellationToken cancellationToken)
    {
        server.ServerStatus = ServerStatus.Online;
        server.LastSeenOnline = DateTimeOffset.Now;

        serverPingResponse.Players.Sample ??= [];

        var serverPlayers = await trackerDatabaseContext.MinecraftPlayerSessions
            .Include(x => x.Player)
            .Where(x => x.IpAddress == server.IpAddress && x.SessionEndDate == null)
            .ToListAsync(cancellationToken);

        await StartNewSessions(server, serverPingResponse, cancellationToken, serverPlayers);

        var onlineUuids = serverPingResponse.Players.Sample.Select(x => x.Id).ToList();

        await UpdatePlayerSessions(serverPlayers, onlineUuids);

        await trackerDatabaseContext.SaveChangesAsync(cancellationToken);
    }

    private async Task StartNewSessions(MinecraftServer server, ServerPingResponse serverPingResponse,
        CancellationToken cancellationToken, List<MinecraftPlayerSessions> serverPlayers)
    {
        foreach (var playerInformation in serverPingResponse.Players.Sample!)
        {
            await CreatePlayerIfNotExists(playerInformation, cancellationToken);
            var session =
                serverPlayers.SingleOrDefault(x => x.Uuid == playerInformation.Id && x.SessionEndDate == null);

            if (session != null) continue;
            session = new MinecraftPlayerSessions
            {
                IpAddress = server.IpAddress,
                Uuid = playerInformation.Id,
                SessionStartDate = DateTimeOffset.Now
            };

            trackerDatabaseContext.MinecraftPlayerSessions.Add(session);
        }
    }
    
    private Task UpdatePlayerSessions(List<MinecraftPlayerSessions> serverPlayers, List<string> onlineUuids)
    {
        foreach (var player in serverPlayers)
        {
            var playerStillOnline = onlineUuids.Contains(player.Uuid);
            var lastSession = serverPlayers.SingleOrDefault(x => x.Uuid == player.Uuid);

            if (lastSession == null)
            {
                logger.LogWarning("Found online player without session. {uuid}", player.Uuid);
                continue;
            }

            player.SessionEndDate = playerStillOnline ? null : DateTimeOffset.Now;
        }
        
        return Task.CompletedTask;
    }

    private async Task CreatePlayerIfNotExists(Player playerInformation, CancellationToken cancellationToken)
    {
        var existingPlayer =
            await trackerDatabaseContext.MinecraftPlayers
                .SingleOrDefaultAsync(x => x.Uuid == playerInformation.Id, cancellationToken: cancellationToken);

        if (existingPlayer != null)
        {
            existingPlayer.PlayerName = existingPlayer.PlayerName != playerInformation.Name
                ? playerInformation.Name
                : existingPlayer.PlayerName;
        }
        else
        {
            var newPlayer = new MinecraftPlayer
            {
                Uuid = playerInformation.Id,
                PlayerName = playerInformation.Name
            };

            trackerDatabaseContext.MinecraftPlayers.Add(newPlayer);
        }
        
        await trackerDatabaseContext.SaveChangesAsync(cancellationToken);
    }
}