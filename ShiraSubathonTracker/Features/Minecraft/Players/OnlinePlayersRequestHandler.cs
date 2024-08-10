using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiraSubathonTracker.DAL;

namespace ShiraSubathonTracker.Features.Minecraft.Players;

public class OnlinePlayersRequestHandler(TrackerDatabaseContext trackerDatabaseContext)
    : IRequestHandler<OnlinePlayersRequest, List<OnlinePlayerResponse>>
{
    public async Task<List<OnlinePlayerResponse>> Handle(OnlinePlayersRequest request, CancellationToken cancellationToken)
    {
        var activeSessions = await trackerDatabaseContext.MinecraftPlayerSessions
            .Include(x => x.Server)
            .Include(x => x.Player)
            .Where(x => x.Server!.CurrentServer)
            .ToListAsync(cancellationToken);

        var onlinePlayers = activeSessions.Select(x => new OnlinePlayerResponse
        {
            PlayerName = x.Player!.PlayerName,
            Uuid = x.Player.Uuid
        }).ToList();

        return onlinePlayers;
    }
}