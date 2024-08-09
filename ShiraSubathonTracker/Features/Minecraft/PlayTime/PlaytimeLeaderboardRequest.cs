using MediatR;

namespace ShiraSubathonTracker.Features.Minecraft.PlayTime;

public class PlaytimeLeaderboardRequest: IRequest<List<PlaytimeResponse>>
{
    public required int PlayerLimit { get; set; }
}