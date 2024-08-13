using Microsoft.EntityFrameworkCore;
using ShiraSubathonTracker.DAL.Entities.Minecraft;
using ShiraSubathonTracker.DAL.Entities.Twitch;
using ShiraSubathonTracker.DAL.Entities.Users;

namespace ShiraSubathonTracker.DAL;

public class TrackerDatabaseContext(DbContextOptions<TrackerDatabaseContext> dbContextOptions)
    : DbContext(dbContextOptions)
{
    public DbSet<MinecraftServer> MinecraftServers { get; set; }
    public DbSet<MinecraftPlayerSessions> MinecraftPlayerSessions { get; set; }
    public DbSet<MinecraftVersion> MinecraftVersions { get; set; }
    public DbSet<MinecraftPlayer> MinecraftPlayers { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<JwtToken> JwtTokens { get; set; }
    public DbSet<SubGift> SubGifts { get; set; }
    public DbSet<Subathon> Subathons { get; set; }
    public DbSet<PlaytimeStatisticsByTimestamp> PlaytimeStatisticsByTimestamps { get; set; }
}