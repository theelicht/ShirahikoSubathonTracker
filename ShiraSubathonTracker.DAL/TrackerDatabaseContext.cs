using Microsoft.EntityFrameworkCore;
using ShiraSubathonTracker.DAL.Entities.Minecraft;

namespace ShiraSubathonTracker.DAL;

public class TrackerDatabaseContext(DbContextOptions<TrackerDatabaseContext> dbContextOptions) : DbContext(dbContextOptions)
{
    public DbSet<MinecraftServer> MinecraftServers { get; set; }
    public DbSet<MinecraftPlayer> MinecraftPlayers { get; set; }
}