using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiraSubathonTracker.DAL;
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
}