using MediatR;
using ShiraSubathonTracker.DAL;

namespace ShiraSubathonTracker.Features.Minecraft.PlayTime;

public class PlaytimeStatisticsRequestHandler(TrackerDatabaseContext trackerDatabaseContext) : IRequestHandler<PlaytimeStatisticsRequest, PlaytimeStatisticsResponse>
{
    public Task<PlaytimeStatisticsResponse> Handle(PlaytimeStatisticsRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}