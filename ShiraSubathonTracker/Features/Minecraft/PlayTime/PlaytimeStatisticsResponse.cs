namespace ShiraSubathonTracker.Features.Minecraft.PlayTime;

public class PlaytimeStatisticsResponse
{
    public List<TotalPlaytimeByTimestamp>? TotalPlaytimeByHourResponse { get; set; }
    public List<TotalPlaytimeByTimestamp>? TotalPlaytimeByDayResponse { get; set; }
    public List<TotalPlaytimePerPlayerByTimestamp>? TotalPlaytimePerPlayerByDayResponse { get; set; }
    public List<TotalPlaytimePerPlayerByTimestamp>? TotalPlaytimePerPlayerByHourResponse { get; set; }
}

public class TotalPlaytimeByTimestamp
{
    public required DateTimeOffset Timestamp { get; set; }
    public required double TotalMinutesPlayed { get; set; }
}

public class TotalPlaytimePerPlayerByTimestamp
{
    public required DateTimeOffset Timestamp { get; set; }
    public required int TotalMinutesPlayed { get; set; }
    public required string PlayerName { get; set; }
    public required string Uuid { get; set; }
}