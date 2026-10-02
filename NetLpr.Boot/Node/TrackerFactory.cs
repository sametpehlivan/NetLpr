using System.Drawing;
using NetLpr.Core;
using NetLpr.Core.Log;
using NetLpr.Core.Services.Tracking;
using NetLpr.Core.Services.Tracking.Plate;
using NetLpr.Core.Values.Request;

namespace NetLpr.Boot.Node
{
    public static class TrackerFactory
    {
        public static ITrackingService CreateTrackerService(ILogger logger, TrackingServiceContext trackingServiceContext)
        {

            return new PlateTrackingService(logger,trackingServiceContext);
        }
    }
}
