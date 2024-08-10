using System.ComponentModel.DataAnnotations;

namespace ShiraSubathonTracker.DAL.Entities.Minecraft;

public class MinecraftPlayer
{
    [Key]
    [MaxLength(50)]
    public required string Uuid { get; set; }

    [MaxLength(50)]
    public required string PlayerName { get; set; }
    
    public List<MinecraftPlayerSessions> PlayerSessions { get; set; }
}