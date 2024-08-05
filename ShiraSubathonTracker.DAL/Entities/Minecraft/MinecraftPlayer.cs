using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShiraSubathonTracker.DAL.Entities.Minecraft;

[PrimaryKey("PK_MinecraftPlayer", nameof(IpAddress), nameof(PlayerName))]
public class MinecraftPlayer
{
    [MaxLength(20)]
    public required string IpAddress { get; set; }
    
    [ForeignKey(nameof(IpAddress))]
    public MinecraftServer? Server { get; set; }
    
    [MaxLength(50)]
    public required string PlayerName { get; set; }
    
    [MaxLength(50)]
    public required string Uuid { get; set; }
    
    public int MinutesOnline { get; set; }
    
    public DateTimeOffset LastOnline { get; set; }
}