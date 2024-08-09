using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShiraSubathonTracker.DAL;

namespace ShiraSubathonTracker.Features.Minecraft.PlayTime;

public class PlaytimeLeaderboardRequestHandler(TrackerDatabaseContext trackerDatabaseContext)
    : IRequestHandler<PlaytimeLeaderboardRequest, List<PlaytimeResponse>>
{
    public async Task<List<PlaytimeResponse>> Handle(PlaytimeLeaderboardRequest request,
        CancellationToken cancellationToken)
    {
        var responses = new List<PlaytimeResponse>();
        
        var sessions = await trackerDatabaseContext.MinecraftPlayerSessions
            .Include(x => x.Server)
            .Include(x => x.Player)
            .Where(x => x.Server!.CurrentServer)
            .ToListAsync(cancellationToken);

        if (sessions.IsNullOrEmpty()) return responses;

        var groupedSessions = sessions.GroupBy(x => x.Uuid);

        foreach (var sessionGroup in groupedSessions)
        {
            var key = sessionGroup.Key;
            
            var playerName = sessionGroup.First().Player!.PlayerName;
            var playtime = sessionGroup.Sum(x =>
            {
                var endDate = x.SessionEndDate ?? DateTimeOffset.Now;
                var timeDifference = endDate - x.SessionStartDate;

                return timeDifference.TotalMinutes;
            });
            
            var response = new PlaytimeResponse
            {
                PlayerName = playerName,
                SessionCount = sessionGroup.Count(),
                MinutesPlayed = (int)playtime
            };
            responses.Add(response);
        }
        
        return responses.OrderByDescending(x => x.MinutesPlayed)
            .ToList();
    }
}