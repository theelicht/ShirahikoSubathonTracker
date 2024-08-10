using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiraSubathonTracker.DAL;

namespace ShiraSubathonTracker.Features.Minecraft.Players;

public class OnlinePlayersRequestHandler(TrackerDatabaseContext trackerDatabaseContext)
    : IRequestHandler<OnlinePlayersRequest, OnlinePlayersResponse>
{
    public async Task<OnlinePlayersResponse> Handle(OnlinePlayersRequest request,
        CancellationToken cancellationToken)
    {
        var activeSessions = await trackerDatabaseContext.MinecraftPlayerSessions
            .Include(x => x.Server)
            .Include(x => x.Player)
            .Where(x => x.Server!.CurrentServer && x.SessionEndDate == null)
            .ToListAsync(cancellationToken);

        var onlinePlayers = activeSessions.DistinctBy(x => x.Player!.Uuid)
            .Select(x => new OnlinePlayerResponse
            {
                PlayerName = x.Player!.PlayerName,
                Uuid = x.Player.Uuid
            }).ToList();

        var response = new OnlinePlayersResponse
        {
            OnlineCount = onlinePlayers.Count,
            OnlinePlayers = onlinePlayers
        };

        return response;
    }
}