using MediatR;

namespace ShiraSubathonTracker.Features.Minecraft.PlayTime;

public class PlaytimeStatisticsRequest : IRequest<PlaytimeStatisticsResponse>
{
    public StatisticsTimeGroupingType StatisticsTimeGroupingType { get; set; } = StatisticsTimeGroupingType.Hours;
}

public enum StatisticsTimeGroupingType
{
    Hours = 0,
    Days = 1
}