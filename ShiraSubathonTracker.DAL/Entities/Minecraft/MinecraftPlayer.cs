using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShiraSubathonTracker.DAL.Entities.Minecraft;

[PrimaryKey(nameof(IpAddress), nameof(Uuid))]
public class MinecraftPlayer
{
    [MaxLength(40)]
    public required string IpAddress { get; set; }
    
    [ForeignKey(nameof(IpAddress))]
    public MinecraftServer? Server { get; set; }
    
    [MaxLength(50)]
    public required string PlayerName { get; set; }
    
    [MaxLength(50)]
    public required string Uuid { get; set; }
    
    public long SecondsOnline { get; set; }
    
    public DateTimeOffset LastSeenOnline { get; set; }

    public required PlayerStatus Status { get; set; }
}

public enum PlayerStatus
{
    Online = 0,
    Offline = 1
}