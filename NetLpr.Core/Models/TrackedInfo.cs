using System.Diagnostics;
using System.Drawing;
using NetLpr.Core.Values.Inference;
using NetLpr.Core.Values.Tracking;
using static System.Net.Mime.MediaTypeNames;

namespace NetLpr.Core.Models
{
    public class TrackedInfo : IDisposable
    {
        public const string PLATES = "Plates";
        public const string CAR_TYPES = "CarTypes";
        public const string REGIONS = "Regions";
        public const string DIRECTION = "Direction";

        private bool _disposed = false;
        public string SourceId { get;  set; }
        public string TrackedId { get;  set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public BaseImage? Image { get;  set; }
        public Dictionary<string, List<ScoreAndValue>> TrackingValues { get; set; } = new();
        public string ImagePath { get; set; } = string.Empty;
        public RectangleF PlateRectangle { get; set; } = RectangleF.Empty;
        public TrackedInfo(string sourceId, string trackedId, Dictionary<string, List<InferenceResult>>? trackedMetaQueues = null, BaseImage? image = null)
        {
            SourceId = sourceId;
            TrackedId = trackedId;
            Image = image;
            Initialize(trackedMetaQueues);
        }
        private void Initialize(Dictionary<string, List<InferenceResult>>? trackedMetaQueues)
        {
            

            if (trackedMetaQueues != null)
            {
                var values = CalculateTrackingValues(trackedMetaQueues);
                foreach (var item in values)
                {
                    TrackingValues.Add(item.Key, item.Value);
                }
            }
            
        }

        private float CalculatePlateGroupScore(int plateLength, int maxPlateLength, int totalRepeatCount, int totalPlateRepaetCount, float avarageScore)
        {
            return (float)plateLength / maxPlateLength * .20f + (float)totalRepeatCount / totalPlateRepaetCount * .60f + avarageScore * .20f;

        }
        private List<ScoreAndValue> GetRegions(Dictionary<string, List<InferenceResult>> trackedMetaQueues)
        {
            trackedMetaQueues.TryGetValue(PLATES, out var plates);
            if (plates == null || plates.Count() == 0) return new();
            var groups = plates
            .Select(x =>
            {
                var region = x.GetInformation<string>("region") ?? string.Empty;
                var confidence = x.GetInformation<float>("region_conf");

                return new ScoreAndValue(region, confidence);
            })
            .Where(x => x.Score != 0 || !string.IsNullOrWhiteSpace(x.Value))
            .GroupBy(x => x.Value)
            .Select(x => new ScoreAndValue(
                x.Key,
                x.Average(p => p.Score)))
            .Where(x => x.Score >= .5f)
            .OrderByDescending(x => x.Score)
            .ToList();
            return groups;
        }
        private List<ScoreAndValue> GetPlates(Dictionary<string, List<InferenceResult>> trackedMetaQueues)
        {
            trackedMetaQueues.TryGetValue(PLATES, out var plates);
            if (plates == null || plates.Count() == 0) return new();
            var maxPlateLength = plates.Select(x => x.Label).OrderByDescending(x => x.Length).First().Length;
            int minPlateLength = 6;
            int plateLengthLowerLimit = maxPlateLength - 2;
            (int maxLength, List<InferenceResult> plateFiltered) = (0, new List<InferenceResult>());
            if (maxPlateLength < minPlateLength)
                (maxLength, plateFiltered) = (maxPlateLength, plates.Where(x => x.Label.Length >= plateLengthLowerLimit).ToList());
            else
                (maxLength, plateFiltered) = (maxPlateLength, plates.Where(x => x.Label.Length >= maxPlateLength).ToList());
            if (plateFiltered.Count() > 0)
            {
                var groups = plateFiltered.GroupBy(x => x.Label).Where(x => x.Average(x => x.Score) > 0.5f);
                if (groups.Count() > 0)
                {
                    return groups
                      .OrderByDescending(g => CalculatePlateGroupScore(g.Key.Length, maxLength, g.Count(), plateFiltered.Count(), g.Average(x => x.Score) * .20f))
                      .Select(g => new ScoreAndValue(g.Key, CalculatePlateGroupScore(g.Key.Length, maxLength, g.Count(), plateFiltered.Count(), g.Average(x => x.Score) * .20f)))
                      .ToList();
                }
            }
            return new();
        }

        private List<ScoreAndValue> GetVehicleTypes(Dictionary<string, List<InferenceResult>> trackedMetaQueues)
        {
            trackedMetaQueues.TryGetValue(CAR_TYPES, out var carTypes);
            List<ScoreAndValue>? defaultCartype = null;
            if (carTypes != null && carTypes.Count() > 0)
                defaultCartype = carTypes
                .GroupBy(x => x.GetLabel())
                .OrderByDescending(g => g.Count())
                .Select(g => new ScoreAndValue(g.Key, g.Average(x => x.Score)))
                .ToList();

            return defaultCartype ?? new();
        }
        private List<ScoreAndValue> GetDirection(Dictionary<string, List<InferenceResult>> trackedMetaQueues)
        {
            float X = 0;
            float Y = 0;
            trackedMetaQueues.TryGetValue(PLATES, out var plates);
            if (plates != null && plates.Count >= 2)
            {
                var current = plates[plates.Count - 1];
                var before = plates[0];
                X += current.X1 - before.X1;
                Y += current.Y1 - before.Y1;
            }
            var val = new List<ScoreAndValue>() { new ScoreAndValue("x", X), new ScoreAndValue("y", Y) };
            return val;
        }

        private Dictionary<string, List<ScoreAndValue>> CalculateTrackingValues(Dictionary<string, List<InferenceResult>> trackedMetaQueues)
        {
            return new Dictionary<string, List<ScoreAndValue>>
            {

                { PLATES, GetPlates(trackedMetaQueues) },
                { CAR_TYPES, GetVehicleTypes(trackedMetaQueues) },
                { REGIONS, GetRegions(trackedMetaQueues) },
                { DIRECTION, GetDirection(trackedMetaQueues)}

            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Image?.Dispose();
            }
            _disposed = true;
        }
    }
}