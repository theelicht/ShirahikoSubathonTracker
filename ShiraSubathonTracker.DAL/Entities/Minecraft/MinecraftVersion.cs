using System.ComponentModel.DataAnnotations;

namespace ShiraSubathonTracker.DAL.Entities.Minecraft;

public class MinecraftVersion
{
    [Key]
    [MaxLength(20)]
    public required string Version { get; set; }

    // http://wiki.vg/Server_List_Ping#Ping_Process
    public required int ServerProtocol { get; set; }

    public List<MinecraftServer> Servers { get; set; } = [];
}