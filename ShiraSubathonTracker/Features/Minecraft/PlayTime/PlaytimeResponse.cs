namespace ShiraSubathonTracker.Features.Minecraft.PlayTime;

public class PlaytimeResponse
{
    public required string PlayerName { get; set; }
    public required int SessionCount { set; get; }
    public required int MinutesPlayed { get; set; }
}