using System;
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
    [Function("ServerPing")]
    public async Task RunAsync([TimerTrigger("*/15 * * * * *")] TimerInfo myTimer, FunctionContext context)
    {
        var servers = await trackerDatabaseContext.MinecraftServers
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

        await EstablishConnection(server, task, tpcClient, cancellationToken);

        await using var stream = tpcClient.GetStream();
        await SetupHandshake(stream, host, port);

        SendStatusRequest(stream);

        var response = await ReadStreamData(stream, cancellationToken);
        await StoreServerData(server, response, cancellationToken);
    }

    private async Task EstablishConnection(MinecraftServer server, ValueTask task, TcpClient tpcClient,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Attempting connection to {serverAddress}...", server.IpAddress);
            await AwaitConnection(task);
        }
        catch (TimeoutException e)
        {
            server.ServerStatus = ServerStatus.ConnectionTimedOut;
            await trackerDatabaseContext.SaveChangesAsync(cancellationToken);

            logger.LogError(e, "Connection to {serverAddress} timed out!", server.IpAddress);
            throw;
        }

        if (!tpcClient.Connected)
        {
            server.ServerStatus = ServerStatus.Offline;
            await trackerDatabaseContext.SaveChangesAsync(cancellationToken);

            logger.LogError("Could not establish connection to {serverAddress}.", server.IpAddress);
            throw new ConnectionAbortedException();
        }
    }

    private static Task AwaitConnection(ValueTask task)
    {
        const long timeOut = 1000;
        var startingTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        while (!task.IsCompleted)
        {
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (currentTime - startingTime >= timeOut)
            {
                throw new TimeoutException();
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Send a "Handshake" packet.
    /// http://wiki.vg/Server_List_Ping#Ping_Process
    /// </summary>
    /// <param name="stream">Stream used to set up handshake.</param>
    /// <param name="serverHost">The server host to connect to.</param>
    /// <param name="serverPort">The server port used for the connection.</param>
    private Task SetupHandshake(NetworkStream stream, string serverHost, int serverPort)
    {
        logger.LogInformation("Attempting handshake.");
        // TODO: Replace with dynamic value set in database, current value = v1.21
        streamBuffer.WriteInt(767);
        streamBuffer.WriteString(serverHost);
        streamBuffer.WriteShort((short)serverPort);
        streamBuffer.WriteInt(1);

        streamBuffer.FlushToStream(stream);
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
            var length = streamBuffer.ReadInt();
            var packet = streamBuffer.ReadInt();
            var jsonLength = streamBuffer.ReadInt();

            logger.LogInformation("Received packet 0x{packedId} with a length of {length}", packet.ToString("X2"),
                length);

            var json = streamBuffer.ReadString(jsonLength);
            var pingResponse = JsonSerializer.Deserialize<ServerPingResponse>(json);
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
        server.LastOnline = DateTimeOffset.Now;
    }
}