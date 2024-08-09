using MediatR;

namespace ShiraSubathonTracker.Features.Twitch.SubGift;

public class SubGiftListRequest : IRequest<SubGiftResponse>
{
    public IReadOnlyList<SubGiftRequest> Requests { get; set; }
}

public class SubGiftRequest
{
    public required string TwitchUsername { get; set; }
    public required int AmountGifted { get; set; }
}