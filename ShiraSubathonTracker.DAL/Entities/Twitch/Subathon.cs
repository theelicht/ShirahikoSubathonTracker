namespace ShiraSubathonTracker.DAL.Entities.Twitch;

public class Subathon
{
    public int Id { get; set; }
    public bool IsCurrentSubathon { get; set; }
    public List<SubGift> SubGifts { get; set; }
}