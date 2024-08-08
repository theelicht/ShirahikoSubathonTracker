using System.Net.Sockets;
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
        const long timeOut = 1000;
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
        
        var serverPlayers = await trackerDatabaseContext.MinecraftPlayers
            .Where(x => x.IpAddress == server.IpAddress).ToListAsync(cancellationToken);

        foreach (var playerInformation in serverPingResponse.Players.Sample)
        {
            var player = serverPlayers.SingleOrDefault(x => x.Uuid == playerInformation.Id);

            if (player == null)
            {
                player = new MinecraftPlayer
                {
                    IpAddress = server.IpAddress,
                    PlayerName = playerInformation.Name,
                    Uuid = playerInformation.Id,
                    LastSeenOnline = DateTimeOffset.Now,
                    Status = PlayerStatus.Online
                };

                trackerDatabaseContext.MinecraftPlayers.Add(player);
            }

            var now = DateTimeOffset.Now;
            var timeOnline = now - player.LastSeenOnline;
            
            player.SecondsOnline += (int) timeOnline.TotalSeconds;
            player.LastSeenOnline = now;
        }

        var onlineUuids = serverPingResponse.Players.Sample.Select(x => x.Id).ToList();

        foreach (var player in serverPlayers)
        {
            var playerStillOnline = onlineUuids.Contains(player.Uuid);
            player.Status = playerStillOnline ? PlayerStatus.Online : PlayerStatus.Offline;
        }

        await trackerDatabaseContext.SaveChangesAsync(cancellationToken);
    }
}