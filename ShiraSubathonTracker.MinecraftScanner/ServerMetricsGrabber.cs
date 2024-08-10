using System.Text.RegularExpressions;
using ShiraSubathonTracker.DAL.Entities.Minecraft;

namespace ShiraSubathonTracker.MinecraftScanner;

public class ServerMetricsGrabber
{
    public async Task<List<Player>> GrabServerStatistics(string? serverMetricsEndpoint)
    {
        var onlinePlayers = new List<Player>();
        if (serverMetricsEndpoint == null) return onlinePlayers;

        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(serverMetricsEndpoint);

        var metrics = await response.Content.ReadAsStringAsync();
        return ParsePlayerMetrics(metrics);
    }
    
    public static List<Player> ParsePlayerMetrics(string metrics)
    {
        var players = new List<Player>();

        // Regex pattern to match player online metrics
        const string pattern = @"mc_player_online{name=""(?<PlayerName>[^""]+)"",uid=""(?<Uuid>[^""]+)"",} (?<OnlineStatus>[01]\.0)";
        var regex = new Regex(pattern);

        // Find all matches
        var matches = regex.Matches(metrics);

        foreach (Match match in matches)
        {
            var player = new Player
            {
                PlayerName = match.Groups["PlayerName"].Value,
                Uuid = match.Groups["Uuid"].Value,
                IsOnline = match.Groups["OnlineStatus"].Value == "1.0"
            };
            players.Add(player);
        }

        return players;
    }
}