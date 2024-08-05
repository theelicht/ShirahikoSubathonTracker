using Microsoft.AspNetCore.Mvc;

namespace ShiraSubathonTracker.Controllers;

[ApiController]
[Route("/minecraft")]
public class MinecraftServerController: ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMinecraftServerStatus()
    {
        return Ok();
    }
}