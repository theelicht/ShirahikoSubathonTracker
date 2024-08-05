using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ShiraSubathonTracker.MinecraftScanner;

public class ServerPinger(ILogger<ServerPinger> logger)
{
    [Function("ServerPing")]
    public void Run([TimerTrigger("*/15 * * * * *")] TimerInfo myTimer)
    {
        logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        if (myTimer.ScheduleStatus is not null)
        {
            logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            
        }
    }
}