using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Extensions;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.Trackers;

namespace NetLpr.Core.Services.Tracking.Plate
{
    public class PlateBoxTracker : IBoxTracker
    {
        public List<InferenceResult> Plates { get; private set; } = new List<InferenceResult>();
        public PlateBoxTracker(ILogger logger, InferenceResult detection, IKalmanFilter kalmanFilter) : base(logger, detection, kalmanFilter)
        {
        }
        public float GetIoU(InferenceResult detection)
        {
            var targetRect = RectangleF.FromLTRB(detection.X1, detection.Y1, detection.X2, detection.Y2);

            var currentRect = RectangleF.FromLTRB(Last.X1, Last.Y1, Last.X2, Last.Y2);

            float x1 = Math.Max(currentRect.Left, targetRect.Left);
            float y1 = Math.Max(currentRect.Top, targetRect.Top);
            float x2 = Math.Min(currentRect.Right, targetRect.Right);
            float y2 = Math.Min(currentRect.Bottom, targetRect.Bottom);

            float intersectionArea = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
            float currentArea = currentRect.Width * currentRect.Height;
            float targetArea = targetRect.Width * targetRect.Height;

            float unionArea = currentArea + targetArea - intersectionArea;

            return unionArea <= 0 ? 0 : intersectionArea / unionArea;
        }
      
    }
}

