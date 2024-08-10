using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiraSubathonTracker.DAL;
using ShiraSubathonTracker.DAL.Entities.Minecraft;

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
            .Where(x => x.Server!.CurrentServer
                        && (x.SessionStartDate >= trackingStartingDate
                            || x.SessionEndDate >= trackingStartingDate)
            )
            .OrderBy(x => x.SessionStartDate)
            .ToListAsync(cancellationToken);

        var response = new PlaytimeStatisticsResponse
        {
            TotalPlaytimeByHourResponse = request.StatisticsTimeGroupingType == StatisticsTimeGroupingType.Days
                ? null
                : CalculatePlaytimeByTimeRange(trackingStartingDate, playSessions, 1),
            TotalPlaytimeByDayResponse = request.StatisticsTimeGroupingType == StatisticsTimeGroupingType.Hours
                ? null
                : CalculatePlaytimeByTimeRange(trackingStartingDate, playSessions, 24),
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

    private List<TotalPlaytimeByTimestamp> CalculatePlaytimeByTimeRange(DateTimeOffset trackingStartingDate,
        List<MinecraftPlayerSessions> playSessions,
        int hoursBetweenTimestamps)
    {
        var playtimeByTimestamp = PrepareResponseList(trackingStartingDate, hoursBetweenTimestamps);

        var endDate = playtimeByTimestamp.First().Timestamp;
        var startDate = trackingStartingDate;
        var now = DateTimeOffset.Now;

        foreach (var item in playtimeByTimestamp.Select((value, index) => new { index, value }))
        {
            var itemIsInLastPosition = item.index == playtimeByTimestamp.Count - 1;
            var sessionsInRange = playSessions
                .Where(x => x.SessionEndDate >= startDate && !(x.SessionStartDate > endDate))
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

        return playtimeByTimestamp;
    }

    private static List<TotalPlaytimeByTimestamp> PrepareResponseList(DateTimeOffset trackingStartDate,
        int hoursBetweenTimestamps)
    {
        var playtimeByTimestamps = new List<TotalPlaytimeByTimestamp>();

        var currentDate = trackingStartDate;
        var now = DateTimeOffset.Now;
        while (currentDate <= now)
        {
            var newDate = currentDate.AddHours(hoursBetweenTimestamps);
            playtimeByTimestamps.Add(new TotalPlaytimeByTimestamp
            {
                Timestamp = newDate,
                TotalMinutesPlayed = 0
            });
            currentDate = newDate;
        }

        return playtimeByTimestamps;
    }

    private static DateTimeOffset GetSessionStartDate(DateTimeOffset sessionStartDate, DateTimeOffset rangeStartDate)
    {
        return sessionStartDate < rangeStartDate ? rangeStartDate : sessionStartDate;
    }

    private static DateTimeOffset GetSessionEndDate(DateTimeOffset? sessionEndDate, DateTimeOffset rangeEndDate,
        DateTimeOffset now)
    {
        if (sessionEndDate == null) return now;
        return sessionEndDate > rangeEndDate ? rangeEndDate : sessionEndDate.Value;
    }
}