using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiraSubathonTracker.DAL.Entities.Minecraft;

public class MinecraftServer
{
    [Key]
    [MaxLength(40)]
    public required string IpAddress { get; set; }
    
    [MaxLength(200)]
    public string? DnsName { get; set; }
    
    [MaxLength(120)]
    public string? MessageOfTheDay { get; set; }

    public required ServerStatus ServerStatus { get; set; } = ServerStatus.Offline;
    
    public required DateTimeOffset LastSeenOnline { get; set; }
    public bool CurrentServer { get; set; }

    public List<MinecraftPlayerSessions> PlayerSessions { get; set; } = [];
    
    [MaxLength(20)]
    public required string Version { get; set; }
    
    [ForeignKey(nameof(Version))]
    public MinecraftVersion MinecraftVersion { get; set; }

    public string? MetricsEndpoint { get; set; }
}

public enum ServerStatus
{
    Online = 0,
    Offline = 1,
    ConnectionTimedOut = 2
}