using MediatR;

namespace ShiraSubathonTracker.Features.Minecraft.Players;

public class UserStatisticsRequest: IRequest<UserStatisticsResponse>
{
    public required string Username { get; set; }
}