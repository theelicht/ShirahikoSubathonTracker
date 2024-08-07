namespace ShiraSubathonTracker.DAL.Entities.Minecraft;

public class SkinInformation
{
    public required string FilePath { get; set; }
    public required DateTimeOffset LastCacheDate { get; set; }
}