using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ShiraSubathonTracker.Shared;

namespace ShiraSubathonTracker.DAL.Entities.Minecraft;

[PrimaryKey(nameof(IpAddress), nameof(Timestamp), nameof(GroupingType))]
public class PlaytimeStatisticsByTimestamp
{
    [MaxLength(40)]
    public required string IpAddress { get; set; }
    
    [ForeignKey(nameof(IpAddress))]
    public MinecraftServer MinecraftServer { get; set; }
    
    public required DateTimeOffset Timestamp { get; set; }
    
    public required StatisticsTimeGroupingType GroupingType { get; set; }
    
    public required double TotalMinutesPlayed { get; set; }
    
    public required bool Cached { get; set; }
}