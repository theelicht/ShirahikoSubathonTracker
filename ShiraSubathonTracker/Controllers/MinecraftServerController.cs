using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiraSubathonTracker.Features.Minecraft.PlayTime;

namespace ShiraSubathonTracker.Controllers;

[ApiController]
[Authorize]
[Route("/minecraft")]
public class MinecraftServerController(ISender mediator): ControllerBase
{
    [AllowAnonymous]
    [HttpGet("leaderboards")]
    public async Task<IActionResult> GetMinecraftPlaytimeLeaderboards([FromQuery] int limit = 10)
    {
        var response = await mediator.Send(new PlaytimeLeaderboardRequest
        {
            PlayerLimit = limit
        });

        return new OkObjectResult(response);
    }
}