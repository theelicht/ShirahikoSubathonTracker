namespace ShiraSubathonTracker.Features.Minecraft.Players;


public class OnlinePlayersResponse
{
    public required int OnlineCount { get; set; }
    public List<OnlinePlayerResponse> OnlinePlayers { get; set; } = [];
}
public class OnlinePlayerResponse
{
    public required string PlayerName { get; set; }
    public required string Uuid { get; set; }
}