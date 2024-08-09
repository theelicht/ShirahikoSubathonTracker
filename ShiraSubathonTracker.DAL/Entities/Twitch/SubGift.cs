using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShiraSubathonTracker.DAL.Entities.Twitch;

[PrimaryKey(nameof(TwitchUsername), nameof(DateOfGift))]
public class SubGift
{
    [MaxLength(50)]
    public required string TwitchUsername { get; set; }
    public required int AmountGifted { get; set; }
    public required DateTimeOffset DateOfGift { get; set; }
    
    public required int SubathonId { get; set; }
    [ForeignKey(nameof(SubathonId))]
    public Subathon Subathon { get; set; }
}