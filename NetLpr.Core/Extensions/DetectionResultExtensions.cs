using System.Drawing;
using NetLpr.Core.Models;
using NetLpr.Core.Values.Inference;
using NetLpr.Core.Values.Preprocessing;
using NetLpr.Core.Values.Tracking;

namespace NetLpr.Core.Extensions
{
    public static class DetectionResultExtensions
    {
        public static RectangleF ToRectangleF(this InferenceResult detectionResult)
        {
            return new RectangleF(detectionResult.X1, detectionResult.Y1, detectionResult.X2 - detectionResult.X1, detectionResult.Y2 - detectionResult.Y1);
        }
        public static RectangleF Scale(this RectangleF rectangleF, Size sourceSize, Size targetSize)
        {
            float scaleX = (float)targetSize.Width / sourceSize.Width;
            float scaleY = (float)targetSize.Height / sourceSize.Height;

            return new RectangleF(
                rectangleF.X * scaleX,
                rectangleF.Y * scaleY,
                rectangleF.Width * scaleX,
                rectangleF.Height * scaleY
            );
        }
        public static (bool inside, (int x, int y) point) IsPointInPolygon(this InferenceResult detection, List<(int x, int y)> points, CornersOfRectangle cof)
        {
            (int x, int y) p = (0, 0);

            switch (cof)
            {
                case CornersOfRectangle.LT: 
                    p = (detection.X1, detection.Y1); break;
                case CornersOfRectangle.RT: 
                    p = (detection.X2, detection.Y1); break;
                case CornersOfRectangle.LB: 
                    p = (detection.X1, detection.Y2); break;
                case CornersOfRectangle.RB: 
                    p = (detection.X2, detection.Y2); break;
            }

            return (p.IsPointInPolygon(points), p);
        }
        public static (List<CornersOfRectangle> insideCorners, List<CornersOfRectangle> outsideCorners) IsAnyPointOutOfArea(this InferenceResult detection, List<(int x, int y)> points)
        {

            var corners = CornersOfRectangleExtensions.GetCorners();
            var insideCorners = new List<CornersOfRectangle>();
            var outsideCorneds = new List<CornersOfRectangle>();
            corners.ForEach(x =>
            {

                if (detection.IsPointInPolygon(points, x).inside)
                    insideCorners.Add(x);
                else
                    outsideCorneds.Add(x);
            });
            return (insideCorners, outsideCorneds);
        }
        public static List<InferenceResult> CalculateOriginalBox(this List<InferenceResult> detectionResults, Size originalSize, Size processedSize, ScalingPolicy scalingPolicy)
        {
            CalculateOriginalBoxInfo info = new CalculateOriginalBoxInfo(originalSize, processedSize,scalingPolicy);

            foreach (var det in detectionResults)
            {
                det.CalculateAndInitiateOriginalBox(info);
            }
            return detectionResults;
        }
        public static List<InferenceResult> RemoveDuplicateBoxesNoNested(
            this List<InferenceResult> detections,
            float distanceThresh = 30f
        )
        {
            if (detections.Count <= 1)
                return detections;

            var filtered = new List<InferenceResult>();

            foreach (var det in detections)
            {
                bool isDuplicate = false;

                foreach (var existing in filtered)
                {
                    if (det.X1 >= existing.X1 && det.Y1 >= existing.Y1 &&
                        det.X2 <= existing.X2 && det.Y2 <= existing.Y2)
                    {
                        isDuplicate = true;
                        break;
                    }

                  
                    if (existing.X1 >= det.X1 && existing.Y1 >= det.Y1 &&
                        existing.X2 <= det.X2 && existing.Y2 <= det.Y2)
                    {
                        filtered.Remove(existing);
                        break; 
                    }

                    double centerX = (det.X1 + det.X2) / 2f;
                    double centerY = (det.Y1 + det.Y2) / 2f;
                    double exCenterX = (existing.X1 + existing.X2) / 2f;
                    double exCenterY = (existing.Y1 + existing.Y2) / 2f;

                    double distSq = Math.Pow((centerX - exCenterX),2) + Math.Pow((centerY - exCenterY),2);
                    double threshSq = distanceThresh * distanceThresh;

                    if (distSq < threshSq)
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate)
                    filtered.Add(det);
            }

            return filtered;
        }
    }
}
