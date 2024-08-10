using MediatR;

namespace ShiraSubathonTracker.Features.Minecraft.PlayTime;

public class PlaytimeStatisticsRequest : IRequest<PlaytimeStatisticsResponse>
{
    public long StatisticsTrackingStartTimestamp { get; set; } = 1723197600; // Shirahiko subathon starting date
    public StatisticsTimeGroupingType StatisticsTimeGroupingType { get; set; } = StatisticsTimeGroupingType.Both;
}

public enum StatisticsTimeGroupingType
{
    Hours = 0,
    Days = 1,
    Both = 2
}