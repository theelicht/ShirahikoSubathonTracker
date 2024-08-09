using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiraSubathonTracker.DAL;

namespace ShiraSubathonTracker.Features.Twitch.SubGift;

public class SubGiftRequestHandler(TrackerDatabaseContext trackerDatabaseContext)
    : IRequestHandler<SubGiftListRequest, SubGiftResponse>
{
    public async Task<SubGiftResponse> Handle(SubGiftListRequest listRequest, CancellationToken cancellationToken)
    {
        var subathon =
            await trackerDatabaseContext.Subathons.SingleOrDefaultAsync(x => x.IsCurrentSubathon,
                cancellationToken: cancellationToken);

        if (subathon == null) throw new ArgumentNullException(nameof(subathon), "Could not find an active subathon.");

        foreach (var request in listRequest.Requests)
        {
            var subGift = new DAL.Entities.Twitch.SubGift
            {
                TwitchUsername = request.TwitchUsername,
                AmountGifted = request.AmountGifted,
                DateOfGift = DateTimeOffset.Now,
                SubathonId = subathon.Id,
                SubTier = request.SubTier
            };

            trackerDatabaseContext.SubGifts.Add(subGift);
        }

        await trackerDatabaseContext.SaveChangesAsync(cancellationToken);
        return new SubGiftResponse();
    }
}