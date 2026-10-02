using System.Drawing;
using NetLpr.Core.Extensions;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.Tracking;


namespace NetLpr.Core.Services.Trackers
{
    public abstract class IBoxTracker : IDisposable
    {
        protected ILogger _logger;
        protected bool disposed = false;
        public string Id { get; private set; }
        public int Age { get; private set; }
        public int Hits { get; private set; }
        public int HitStreak { get; private set; }
        public int TimeSinceUpdate { get; private set; }
        public BaseImage? Image { get; set; }
        public InferenceResult Last { get; protected set; }
        public InferenceResult First { get; protected set; }
        public IKalmanFilter KalmanFilter { get; protected set; }
        public RectangleF PlateRectangle { get; set; } = RectangleF.Empty;
        public string ImagePath { get; set; } = string.Empty;

        public IBoxTracker(ILogger logger, InferenceResult detection, IKalmanFilter kalmanFilter)
        {
            _logger = logger;
            Id = Guid.NewGuid().ToString();
            First = Last = detection;
            KalmanFilter = kalmanFilter;
            BboxToMeasurement(
                RectangleF.FromLTRB(detection.X1, detection.Y1, detection.X2, detection.Y2),  
                out float x, out float y, out float s, out float r
            );
            KalmanFilter.InitializeState(x, y, s, r);
            Age = 0;
            Hits = 0;
            HitStreak = 0;
            TimeSinceUpdate = 0;
        
           
   
        }
        public void BboxToMeasurement(RectangleF bbox, out float x, out float y, out float s, out float r)
        {
            float w = (float)bbox.Width;
            float h = (float)bbox.Height;
            x = (float)(bbox.X + w * 0.5);
            y = (float)(bbox.Y + h * 0.5);
            s = w * h;
            r = w / h;
        }
        public RectangleF StateToRect(float[] state)
        {
            float x = state[0];
            float y = state[1];
            float s = state[2];
            float r = state[3];

            if (s <= 0 || r <= 0)
            {
                return new RectangleF(x - 1, y - 1, 2, 2);
            }

            float w = (float)Math.Sqrt(s * r);
            float h = s / w;
            return new RectangleF(x - w * 0.5f, y - h * 0.5f, w, h);
        }
        public virtual void Update(InferenceResult detection)
        {
            var bbox = RectangleF.FromLTRB(detection.X1, detection.Y1, detection.X2, detection.Y2);
            try
            {

                if (disposed) throw new ObjectDisposedException(nameof(IBoxTracker));

                TimeSinceUpdate = 0;
                Hits++;
                HitStreak++;

                float x, y, s, r;
                BboxToMeasurement(bbox, out x, out y, out s, out r);
                KalmanFilter.Correct(x, y, s, r);

                Last = detection;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Tracker updating error!");
            }
        }
        public virtual  RectangleF Predict()
        {
            try
            {
                if (disposed) throw new ObjectDisposedException(nameof(IBoxTracker));

                var prediction = KalmanFilter.Predict();

                Age++;
                if (TimeSinceUpdate > 0)
                    HitStreak = 0;
                TimeSinceUpdate++;

                var bbox = StateToRect(prediction);

                return bbox;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Tracker predict error!");

            }
            return new RectangleF();
        }
        public InferenceResult ToDetectionResult()
        {
            if (disposed) throw new ObjectDisposedException(nameof(IBoxTracker));
            return Last;
        }
        public void Dispose()
        {
            if (!disposed)
            {
                KalmanFilter?.Dispose();
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }
    }
}
