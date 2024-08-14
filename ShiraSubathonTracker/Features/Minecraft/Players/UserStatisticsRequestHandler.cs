using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiraSubathonTracker.DAL;
using ShiraSubathonTracker.DAL.Entities.Minecraft;

namespace ShiraSubathonTracker.Features.Minecraft.Players;

public class UserStatisticsRequestHandler(TrackerDatabaseContext trackerDatabaseContext)
    : IRequestHandler<UserStatisticsRequest, UserStatisticsResponse>
{
    public async Task<UserStatisticsResponse> Handle(UserStatisticsRequest request, CancellationToken cancellationToken)
    {
        var player =
            await trackerDatabaseContext.MinecraftPlayers
                .Include(x => x.PlayerSessions)
                .ThenInclude(x => x.Server)
                .SingleOrDefaultAsync(x => x.PlayerName == request.Username, cancellationToken: cancellationToken);

        if (player == null) throw new ArgumentNullException(nameof(player), "User not found.");

        var serversPlayedOn = player.PlayerSessions
            .DistinctBy(x => x.IpAddress)
            .Select(x => x.Server!.DnsName ?? x.Server.IpAddress)
            .ToList();

        var serverStatistics = GetStatisticsPerServer(player, serversPlayedOn);

        return new UserStatisticsResponse
        {
            Username = player.PlayerName,
            Uuid = player.Uuid,
            ServersPlayedOn = serversPlayedOn,
            StatisticsPerServer = serverStatistics
        };
    }

    private List<ServerStatistics> GetStatisticsPerServer(MinecraftPlayer player, List<string> serversPlayedOn)
    {
        var serverStatisticsList = new List<ServerStatistics>();

        foreach (var serverPlayedOn in serversPlayedOn)
        {
            var serverSpecificSessions = player.PlayerSessions
                .Where(x => x.Server!.IpAddress == serverPlayedOn || x.Server.DnsName == serverPlayedOn)
                .OrderBy(x => x.SessionStartDate)
                .ToList();
            
            if (serverSpecificSessions.Count == 0) continue;

            var sessionStatistics = GetSessionStatistics(serverSpecificSessions);
            var totalPlaytime = sessionStatistics.Sum(x => x.TotalMinutesInSession);
            var roundedTotalPlaytime = (int)Math.Round(totalPlaytime);
            
            serverStatisticsList.Add(new ServerStatistics
            {
                ServerName = serverPlayedOn,
                TotalMinutesPlayed = roundedTotalPlaytime,
                IsOnline = serverSpecificSessions.Last().SessionEndDate == null,
                ServerSessions = sessionStatistics
            });
        }

        return serverStatisticsList;
    }

    private List<ServerSession> GetSessionStatistics(List<MinecraftPlayerSessions> serverSpecificSessions)
    {
        var serverSessions = serverSpecificSessions.Select(serverSpecificSession =>
        {
            var endDate = serverSpecificSession.SessionEndDate ?? DateTimeOffset.Now;
            var timeDifference = endDate - serverSpecificSession.SessionStartDate;
            var playtime = timeDifference.TotalMinutes;

            return new ServerSession
            {
                SessionStartDate = serverSpecificSession.SessionStartDate,
                SessionEndDate = serverSpecificSession.SessionEndDate,
                TotalMinutesInSession = playtime
            };
        }).ToList();

        return serverSessions;
    }
}