using System.Drawing;
using NetLpr.Core.Models;

namespace NetLpr.Core.Extensions
{
    public static class InferenceResultExtensions
    {
        public static void CalculateOriginalCoordinate(this InferenceResult? inferenceResult, RectangleF? roi)
        {
            if (inferenceResult == null || roi == null)
                return;
            inferenceResult.X1 += (int)roi.Value.X;
            inferenceResult.X2 += (int)roi.Value.X;
            inferenceResult.Y1 += (int)roi.Value.Y;
            inferenceResult.Y2 += (int)roi.Value.Y;
        }
        public static void CalculateOriginalCoordinate(this List<InferenceResult>? inferenceResults, RectangleF? roi)
        {
            if (inferenceResults == null || inferenceResults.Count == 0 || roi == null)
                return;
            inferenceResults.ForEach(inferenceResult =>
            {
                inferenceResult.X1 += (int)roi.Value.X;
                inferenceResult.X2 += (int)roi.Value.X;
                inferenceResult.Y1 += (int)roi.Value.Y;
                inferenceResult.Y2 += (int)roi.Value.Y;
            });
        }
    }
}
