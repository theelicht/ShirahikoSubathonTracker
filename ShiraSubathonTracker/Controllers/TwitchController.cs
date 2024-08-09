using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiraSubathonTracker.DAL;
using ShiraSubathonTracker.DAL.Entities.Twitch;
using ShiraSubathonTracker.Features.Twitch.SubGift;

namespace ShiraSubathonTracker.Controllers;

[Authorize]
[ApiController]
[Route("twitch")]
public class TwitchController(TrackerDatabaseContext context, ISender mediator): ControllerBase
{
    [HttpPost("subGift")]
    public async Task<IActionResult> GiftSub([FromBody] SubGiftListRequest request)
    {
        try
        {
            var response = await mediator.Send(request);
            return Created();
        }
        catch (ArgumentNullException e)
        {
            return NotFound("Could not find an active subathon to add sub gifts to.");
        }
    }

    [AllowAnonymous]
    [HttpGet("gifts/leaderboards")]
    public async Task<IActionResult> GetLeaderboards()
    {
        var sessions = await context.SubGifts
            .Include(x => x.Subathon)
            .Where(x => x.Subathon.IsCurrentSubathon)
            .GroupBy(x => x.TwitchUsername)
            .ToListAsync();

        var mapping = sessions.Select(MapGroup).OrderByDescending(x => x.AmountGifted);

        return new OkObjectResult(mapping);
    }

    private dynamic MapGroup(IGrouping<string, SubGift> group)
    {
        {
            var groupKey = group.Key;
            var sumOfGifts = group.Sum(x => x.AmountGifted);
            var username = group.First().TwitchUsername;

            return new
            {
                Username = username,
                AmountGifted = sumOfGifts
            };
        }
    }
}