namespace ShiraSubathonTracker.Features.Minecraft.Players;

public class UserStatisticsResponse
{
    public required string Username { get; set; }
    public required string Uuid { get; set; }
    public required List<string> ServersPlayedOn { get; set; }
    public required List<ServerStatistics> StatisticsPerServer { get; set; }

}

public class ServerStatistics
{
    public required string ServerName { get; set; } // Either IP or DNS name
    public required long TotalMinutesPlayed { get; set; }
    public required bool IsOnline { get; set; }
    public required List<ServerSession> ServerSessions { get; set; }
}

public class ServerSession
{
    public required DateTimeOffset SessionStartDate { get; set; }
    public required DateTimeOffset? SessionEndDate { get; set; }
    public required double TotalMinutesInSession { get; set; }
}
