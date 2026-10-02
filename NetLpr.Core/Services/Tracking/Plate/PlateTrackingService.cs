using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using NetLpr.Core.Datas;
using NetLpr.Core.Extensions;
using NetLpr.Core.Log;
using NetLpr.Core.Models;


namespace NetLpr.Core.Services.Tracking.Plate
{
    public class PlateTrackingService : ITrackingService
    {
        private const float IOU_THRESHOLD = 0.15f;
        private const float MAX_CENTER_DISTANCE_PIXELS = 120.0f; 
        private const double OCR_SIMILARITY_THRESHOLD = 80.0;  
        private const int MAX_AGE = 7;

        protected List<PlateBoxTracker> _trackers = new();

        public PlateTrackingService(ILogger logger, TrackingServiceContext trackingServiceContext)
            : base(logger, trackingServiceContext)
        {
        }

        public override void OnDispose()
        {
            _trackers.ForEach(x => x.Dispose());
            _trackers.Clear();
        }



        protected override void Update(InferencedInput input, List<(int x, int y)> points)
        {

            var filteredPlates = input.DetectionResults.Where(x =>
            {
         
              
                var plateOcr = x.GetInformation<InferenceResult>(InferenceResult.INFORMATION_PLATE_OCR_INFERENCE_KEY);
                return plateOcr != null
                       && plateOcr.Label.Replace("_", string.Empty).Length >= 5
                       && plateOcr.Score >= 0.8f; 
            }).ToList();

            foreach (var tracker in _trackers)
            {
                tracker.Predict();
            }

            HashSet<int> matchedDetectionIndices = new();
            HashSet<PlateBoxTracker> matchedTrackers = new();

            MatchGreedy(filteredPlates, matchedDetectionIndices, matchedTrackers, (tracker, detection) =>
            {
                float iou = tracker.GetIoU(detection);
                return iou >= IOU_THRESHOLD ? iou : -1f;
            });

            MatchGreedy(filteredPlates, matchedDetectionIndices, matchedTrackers, (tracker, detection) =>
            {
                float distance = GetCenterDistance(tracker, detection);
                if (distance <= MAX_CENTER_DISTANCE_PIXELS)
                {
                    return MAX_CENTER_DISTANCE_PIXELS - distance;
                }
                return -1f;
            });

            MatchGreedy(filteredPlates, matchedDetectionIndices, matchedTrackers, (tracker, detection) =>
            {
                var newOcr = detection.GetInformation<InferenceResult>(InferenceResult.INFORMATION_PLATE_OCR_INFERENCE_KEY);
                var lastDetection = tracker.Last;
                var lastOcr = lastDetection?.GetInformation<InferenceResult>(InferenceResult.INFORMATION_PLATE_OCR_INFERENCE_KEY);

                if (newOcr != null && lastOcr != null)
                {
                    string text1 = lastOcr.Label.Replace("_", string.Empty);
                    string text2 = newOcr.Label.Replace("_", string.Empty);

                    double similarity = PlateSimilarityCalculator.CalculateWeightedSimilarity(text1, text2);
                    if (similarity >= OCR_SIMILARITY_THRESHOLD)
                    {
                        return (float)similarity;
                    }
                }
                return -1f;
            });

            for (int i = 0; i < filteredPlates.Count; i++)
            {
                if (!matchedDetectionIndices.Contains(i))
                {
                    var detection = filteredPlates[i];


                    if (IsInAreaAnyPoint(detection,points))
                    {
                        IKalmanFilter kalmanFilter = new EmptyKalmanFilter();
                        var newTracker = new PlateBoxTracker(_logger, detection, kalmanFilter);
                        if (newTracker.Image == null)
                        {
                            AddImage(input.Image, newTracker, DEFAULT_TRACKING_SNAPSHOT_RESULT_SIZE, plateDetection: detection);
                        }
                        _trackers.Add(newTracker);
                        newTracker.Plates.Add(detection);
                    }

                }
            }

            for (int i = _trackers.Count - 1; i >= 0; i--)
            {
                var tracker = _trackers[i];

                if (_trackers[i].TimeSinceUpdate > MAX_AGE || !IsInAreaAnyPoint(tracker.Last, points))
                {

                    _trackers.RemoveAt(i);

                    if (tracker.Image == null)
                    {
                        AddImage(input.Image, tracker, DEFAULT_TRACKING_SNAPSHOT_RESULT_SIZE, tracker.ToDetectionResult());
                    }

                    var plates = new List<InferenceResult>();
                    foreach (var item in tracker.Plates)
                    {
                        var plateOcr = item.GetInformation<InferenceResult>(InferenceResult.INFORMATION_PLATE_OCR_INFERENCE_KEY);
                        if (plateOcr != null)
                        {
                            plateOcr.X1 = item.X1;
                            plateOcr.Y1 = item.Y1;
                            plateOcr.X2 = item.X2;
                            plateOcr.Y2 = item.Y2;
                            plates.Add(plateOcr);
                        }
                    }
                    var trackedInfo = new TrackedInfo(input.SourceId, tracker.Id, new() { { TrackedInfo.PLATES,plates} },tracker.Image);
                    trackedInfo.PlateRectangle = tracker.PlateRectangle;
                    trackedInfo.ImagePath = tracker.ImagePath;
                    _trackingServiceContext.Enqueue(_serviceId, trackedInfo);
                    tracker.Image = null;
                    tracker.Dispose();
                }
            }
        }


        private void MatchGreedy(
            List<InferenceResult> detections,
            HashSet<int> matchedDetectionIndices,
            HashSet<PlateBoxTracker> matchedTrackers,
            Func<PlateBoxTracker, InferenceResult, float> scoreEvaluator)
        {
            while (true)
            {
                float bestScore = 0f;
                int bestDetectionIdx = -1;
                PlateBoxTracker? bestTracker = null;

                for (int i = 0; i < detections.Count; i++)
                {
                    if (matchedDetectionIndices.Contains(i)) continue;

                    var detection = detections[i];

                    foreach (var tracker in _trackers)
                    {
                        if (matchedTrackers.Contains(tracker)) continue;

                        float score = scoreEvaluator(tracker, detection);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestDetectionIdx = i;
                            bestTracker = tracker;
                        }
                    }
                }

                if (bestTracker == null || bestDetectionIdx == -1)
                    break;

                bestTracker.Update(detections[bestDetectionIdx]);
                bestTracker.Plates.Add(detections[bestDetectionIdx]);
                matchedTrackers.Add(bestTracker);
                matchedDetectionIndices.Add(bestDetectionIdx);
            }
        }

        private float GetCenterDistance(PlateBoxTracker tracker, InferenceResult detection)
        {

            var trackBox = tracker.ToDetectionResult().ToRectangleF();
            var detBox = detection.ToRectangleF();

            float trackCenterX = trackBox.X + (trackBox.Width / 2.0f);
            float trackCenterY = trackBox.Y + (trackBox.Height / 2.0f);

            float detCenterX = detBox.X + (detBox.Width / 2.0f);
            float detCenterY = detBox.Y + (detBox.Height / 2.0f);

            float dx = trackCenterX - detCenterX;
            float dy = trackCenterY - detCenterY;

            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        public bool IsInAreaAnyPoint(InferenceResult detection, List<(int x, int y)> points)
        {
            var isInAreaAnyPoint = true;
            if (points.Count > 0)
                isInAreaAnyPoint = detection.GetPoints().Any(x => x.IsPointInPolygonD(points));
            return isInAreaAnyPoint;
        }
    }
     
}