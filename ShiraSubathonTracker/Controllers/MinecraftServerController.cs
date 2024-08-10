using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiraSubathonTracker.Features.Minecraft.Players;
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

    [AllowAnonymous]
    [HttpGet("players/online")]
    public async Task<IActionResult> GetOnlinePlayers()
    {
        var onlinePlayers = await mediator.Send(new OnlinePlayersRequest());
        return new OkObjectResult(onlinePlayers);
    }
    
    [AllowAnonymous]
    [HttpGet("statistics")]
    public async Task<IActionResult> GetPlaytimeStatistics()
    {
        var statisticsResponse = await mediator.Send(new PlaytimeStatisticsRequest());
        return new OkObjectResult(statisticsResponse);
    }
}