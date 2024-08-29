using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiraSubathonTracker.DAL;
using ShiraSubathonTracker.DAL.Entities.Minecraft;
using ShiraSubathonTracker.Shared;

namespace ShiraSubathonTracker.Features.Minecraft.PlayTime;

public class PlaytimeStatisticsRequestHandler(TrackerDatabaseContext trackerDatabaseContext)
    : IRequestHandler<PlaytimeStatisticsRequest, PlaytimeStatisticsResponse>
{
    public async Task<PlaytimeStatisticsResponse> Handle(PlaytimeStatisticsRequest request,
        CancellationToken cancellationToken)
    {
        var trackingStartingDate = DateTimeOffset.FromUnixTimeSeconds(request.StatisticsTrackingStartTimestamp);

        var playSessions = await trackerDatabaseContext.MinecraftPlayerSessions
            .AsNoTracking()
            .Include(x => x.Server)
            .Include(x => x.Player)
            .Where(x => x.Server!.CurrentServer)
            .OrderBy(x => x.SessionStartDate)
            .ToListAsync(cancellationToken);

        var response = new PlaytimeStatisticsResponse
        {
            TotalPlaytimeByHourResponse = request.StatisticsTimeGroupingType == StatisticsTimeGroupingType.Days
                ? null
                : await CalculatePlaytimeByTimeRange(trackingStartingDate, playSessions, 1,
                    StatisticsTimeGroupingType.Hours,
                    cancellationToken),
            TotalPlaytimeByDayResponse = request.StatisticsTimeGroupingType == StatisticsTimeGroupingType.Hours
                ? null
                : await CalculatePlaytimeByTimeRange(trackingStartingDate, playSessions, 24,
                    StatisticsTimeGroupingType.Days,
                    cancellationToken),
            TotalPlaytimePerPlayerByHourResponse = new List<TotalPlaytimePerPlayerByTimestamp>
            {
                new()
                {
                    Timestamp = DateTimeOffset.Now,
                    TotalMinutesPlayed = 999999,
                    PlayerName = "Placeholder",
                    Uuid = "Placeholder"
                }
            },
            TotalPlaytimePerPlayerByDayResponse = new List<TotalPlaytimePerPlayerByTimestamp>
            {
                new()
                {
                    Timestamp = DateTimeOffset.Now,
                    TotalMinutesPlayed = 999999,
                    PlayerName = "Placeholder",
                    Uuid = "Placeholder"
                }
            }
        };

        return response;
    }

    private async Task<List<TotalPlaytimeByTimestamp>> CalculatePlaytimeByTimeRange(DateTimeOffset trackingStartingDate,
        List<MinecraftPlayerSessions> playSessions,
        int hoursBetweenTimestamps,
        StatisticsTimeGroupingType groupingType, CancellationToken cancellationToken)
    {
        var ipAddress = playSessions.First().IpAddress;
        var playtimeByTimestamp =
            await PrepareResponseList(trackingStartingDate, hoursBetweenTimestamps, groupingType, ipAddress,
                cancellationToken);

        var endDate = playtimeByTimestamp.First().Timestamp;
        var startDate = trackingStartingDate;
        var now = DateTimeOffset.UtcNow;

        foreach (var item in playtimeByTimestamp.Select((value, index) => new { index, value }))
        {
            if (item.value.IsCached)
            {
                startDate = playtimeByTimestamp[item.index].Timestamp;
                endDate = playtimeByTimestamp[item.index + 1].Timestamp;
                continue;
            };
            var itemIsInLastPosition = item.index == playtimeByTimestamp.Count - 1;
            var sessionsInRange = playSessions
                .Where(x => (x.SessionEndDate >= startDate && x.SessionEndDate <= endDate) ||
                            (x.SessionEndDate == null && x.SessionStartDate <= endDate) || 
                            (x.SessionEndDate >= endDate && x.SessionStartDate <= endDate)
                )
                .ToList();

            var minutesPlayedInRange = 0D;

            foreach (var session in sessionsInRange)
            {
                var sessionStartingDate = GetSessionStartDate(session.SessionStartDate, startDate);
                var sessionEndDate = GetSessionEndDate(session.SessionEndDate, endDate, now);

                var timeDifference = (sessionEndDate - sessionStartingDate).TotalMinutes;
                minutesPlayedInRange += timeDifference;
            }

            item.value.TotalMinutesPlayed = (int)Math.Round(minutesPlayedInRange);

            if (itemIsInLastPosition) continue;
            startDate = playtimeByTimestamp[item.index].Timestamp;
            endDate = playtimeByTimestamp[item.index + 1].Timestamp;
        }

        // The end date for caching is the start date of the last entry
        await CacheItems(playtimeByTimestamp, ipAddress, groupingType, startDate);
        await trackerDatabaseContext.SaveChangesAsync(cancellationToken);

        return playtimeByTimestamp;
    }

    private Task CacheItems(List<TotalPlaytimeByTimestamp> playtimeByTimestamp, string serverIp,
        StatisticsTimeGroupingType groupingType, DateTimeOffset endDate)
    {
        var itemsToBeCached = playtimeByTimestamp.Where(x => !x.IsCached && x.Timestamp <= endDate).ToList();
        foreach (var itemToBeCached in itemsToBeCached)
        {
            trackerDatabaseContext.PlaytimeStatisticsByTimestamps.Add(new PlaytimeStatisticsByTimestamp
            {
                IpAddress = serverIp,
                Timestamp = itemToBeCached.Timestamp,
                GroupingType = groupingType,
                TotalMinutesPlayed = itemToBeCached.TotalMinutesPlayed,
                Cached = true
            });
        }

        return Task.CompletedTask;
    }

    private async Task<List<TotalPlaytimeByTimestamp>> PrepareResponseList(DateTimeOffset trackingStartDate,
        int hoursBetweenTimestamps, StatisticsTimeGroupingType groupingType, string ipAddress,
        CancellationToken cancellationToken)
    {
        var playtimeByTimestamps = new List<TotalPlaytimeByTimestamp>();
        var cachedPlaytimes = await trackerDatabaseContext.PlaytimeStatisticsByTimestamps
            .AsNoTracking()
            .Where(x => x.Timestamp >= trackingStartDate)
            .ToListAsync(cancellationToken: cancellationToken);

        var currentDate = trackingStartDate;
        var now = DateTimeOffset.Now;

        while (currentDate <= now)
        {
            var newDate = currentDate.AddHours(hoursBetweenTimestamps);
            var cachedPlaytime = cachedPlaytimes.Where(
                    x => x.IpAddress == ipAddress && x.GroupingType == groupingType && x.Timestamp == newDate)
                .Select(x => new TotalPlaytimeByTimestamp
                {
                    Timestamp = x.Timestamp,
                    TotalMinutesPlayed = x.TotalMinutesPlayed,
                    IsCached = true
                }).SingleOrDefault();

            if (cachedPlaytime != null)
            {
                playtimeByTimestamps.Add(cachedPlaytime);
            }
            else
            {
                playtimeByTimestamps.Add(new TotalPlaytimeByTimestamp
                {
                    Timestamp = newDate,
                    TotalMinutesPlayed = 0,
                    IsCached = false
                });
            }

            currentDate = newDate;
        }

        return playtimeByTimestamps.OrderBy(x => x.Timestamp).ToList();
    }

    private static DateTimeOffset GetSessionStartDate(DateTimeOffset sessionStartDate, DateTimeOffset rangeStartDate)
    {
        return sessionStartDate < rangeStartDate ? rangeStartDate : sessionStartDate;
    }

    private static DateTimeOffset GetSessionEndDate(DateTimeOffset? sessionEndDate, DateTimeOffset rangeEndDate,
        DateTimeOffset now)
    {
        return sessionEndDate switch
        {
            null when rangeEndDate >= now => now,
            null when rangeEndDate < now => rangeEndDate,
            _ => sessionEndDate > rangeEndDate ? rangeEndDate : sessionEndDate.Value
        };
    }
}