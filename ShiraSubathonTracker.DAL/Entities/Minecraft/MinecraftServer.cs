using System.ComponentModel.DataAnnotations;

namespace ShiraSubathonTracker.DAL.Entities.Minecraft;

public class MinecraftServer
{
    [Key]
    [MaxLength(20)]
    public required string IpAddress { get; set; }
    
    [MaxLength(200)]
    public string? DnsName { get; set; }
    
    [MaxLength(120)]
    public string? MessageOfTheDay { get; set; }

    public required ServerStatus ServerStatus { get; set; } = ServerStatus.Offline;
    
    public required DateTimeOffset LastOnline { get; set; }
    public bool CurrentServer { get; set; }

    public List<MinecraftPlayer> Players { get; set; } = [];
}

public enum ServerStatus
{
    Online = 0,
    Offline = 1,
    ConnectionTimedOut = 2
}