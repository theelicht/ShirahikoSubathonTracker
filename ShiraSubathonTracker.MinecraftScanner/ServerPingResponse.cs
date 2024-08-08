namespace ShiraSubathonTracker.MinecraftScanner;

public class ServerPingResponse
{
    /// <summary>
    /// Protocol that the server is using and the given name
    /// </summary>
    public VersionPayload Version { get; set; }
    public PlayersPayload Players { get; set; }
    public string Motd { get; set; }

    /// <summary>
    /// Server icon, important to note that it's encoded in base 64
    /// </summary>
    public string Icon { get; set; }
}

public class VersionPayload
{
    public int Protocol { get; set; }
    public string Name { get; set; }
}

public class PlayersPayload
{
    public int Max { get; set; }
    public int Online { get; set; }
    public List<Player>? Sample { get; set; }
}

public class Player
{
    public string Name { get; set; }
    public string Id { get; set; }
}