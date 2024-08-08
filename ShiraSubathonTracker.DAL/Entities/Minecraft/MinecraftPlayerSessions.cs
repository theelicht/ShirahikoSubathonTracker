using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShiraSubathonTracker.DAL.Entities.Minecraft;

[PrimaryKey(nameof(IpAddress), nameof(Uuid), nameof(SessionStartDate))]
public class MinecraftPlayerSessions
{
    [MaxLength(40)]
    public required string IpAddress { get; set; }
    
    [ForeignKey(nameof(IpAddress))]
    public MinecraftServer? Server { get; set; }
    
    [MaxLength(50)]
    public required string Uuid { get; set; }
    
    [ForeignKey(nameof(Uuid))]
    public MinecraftPlayer? Player { get; set; }
    
    public required DateTimeOffset SessionStartDate { get; set; }
    
    public DateTimeOffset? SessionEndDate { get; set; }
}