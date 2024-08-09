using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShiraSubathonTracker.Controllers;

[ApiController]
[Authorize]
[Route("/minecraft")]
public class MinecraftServerController: ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMinecraftServerStatus()
    {
        return Ok();
    }
}