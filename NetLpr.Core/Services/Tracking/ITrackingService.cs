using System.Drawing;
using NetLpr.Core.Datas;
using NetLpr.Core.Extensions;
using NetLpr.Core.Helpers;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.Trackers;
using NetLpr.Core.Services.Tracking.Plate;


namespace NetLpr.Core.Services.Tracking
{
    public abstract class ITrackingService : IDisposable
    {
        public static readonly Size DEFAULT_TRACKING_SNAPSHOT_RESULT_SIZE = new Size(600, 400);

        private volatile bool _disposed;
        private object _syncLock = new object();
        protected string _serviceId;
        protected ILogger _logger;
        protected TrackingServiceContext _trackingServiceContext;
        private EventHandler<TrackedInfo> _eventHandler = (_, _) => { };
   
        public ITrackingService(ILogger logger, TrackingServiceContext trackingServiceContext)
        {
            _disposed = false;
            _logger = logger;
            _serviceId = $"TrackingService-{Guid.NewGuid().ToString()}";
            _trackingServiceContext = trackingServiceContext;
            _trackingServiceContext.Register(_serviceId, Invoke);

        }
        private void Invoke(TrackedInfo input)
        {
            if (_eventHandler.GetInvocationList().Count() == 1)
            {
                input.Dispose();
            }
            else 
                _eventHandler.Invoke(this,input);
        }
        public bool IsDisposed()
        {
            return _disposed;
        }
        protected abstract void Update(InferencedInput input,List<(int x,int y)> points);
        
        public void UpdateTracks(InferencedInput input)
        {
            if (!IsDisposed())
            {
                
                var points = input.StreamAnalysesInfo.AnalysesPoints.Select(pointf =>
                {
                    int x = (int)(pointf.X * input.Image.GetSize().Width);
                    int y = (int)(pointf.Y * input.Image.GetSize().Height);
                    return (x, y);
                }).ToList();
                Update(input, points);
            }
            else _logger.LogInformation($"{this.GetType().Name} frame disposed");
        }
        public void OnFrameReading(object? sender,InferencedInput eventData)
        {
            if (!IsDisposed())
            {
            
                lock (_syncLock)
                {
                    if (!IsDisposed())
                    {
                
                        UpdateTracks(eventData);
                    }
                   
                }
            }
            eventData.Dispose();
        }
    
        public void Subscribe(EventHandler<TrackedInfo> eventHandler)
        {
            if (eventHandler == null)
                return;

            if (IsDisposed())
            {
                _logger.LogError($"{nameof(ITrackingService)} disposed");
                return;
            }
            _eventHandler += eventHandler;

        }
        public void Unsubscribe(EventHandler<TrackedInfo> eventHandler)
        {
            if (eventHandler == null)
                return;

            if (IsDisposed())
            {
                _logger.LogError($"{nameof(ITrackingService)} disposed");
                return;
            }
            _eventHandler -= eventHandler;

        }    
        public void Dispose()
        {
            if (IsDisposed()) return;
            lock (_syncLock)
            {
                try
                {
                    if (IsDisposed()) return;

                    _trackingServiceContext.Unregister(_serviceId);
                    OnDispose();
                    _disposed = true;
                    GC.SuppressFinalize(this);
                    _logger.LogInformation($"{GetType().Name} frame disposed");
                }
                catch(Exception e)
                {
                    _logger.LogError(e,$"{GetType().Name}: ");
                }

            }
        }
        protected string SaveImage(BaseImage image,string sourceId,string trackId)
        {
            if (image != null)
            {
                var directoryBasePath = $"./images/{DateTime.UtcNow.ToString("dd-MM-yyyy")}/{sourceId}";
                var directory = Path.GetFullPath($"{directoryBasePath}");
                var imageSavePath = $"{directory}/{trackId}.jpeg";
                DirectoryCreator.Create(directory);

                if (image.Save(imageSavePath))
                {
                    return $"{directoryBasePath}/{trackId}.jpeg";
                }
            }
            return string.Empty;
        }
        protected void AddImage(BaseImage? snapshot, IBoxTracker tracker,Size newSize, InferenceResult? vehicleDetection = null,InferenceResult? plateDetection = null)
        {
            if (snapshot != null)
            {
                tracker.Image = snapshot.CropAndResizeAndDrawDetectionResults(newSize, new List<InferenceResult?>() { vehicleDetection,plateDetection });
                if(plateDetection != null)
                {
                    tracker.PlateRectangle = plateDetection.ToRectangleF().Scale(snapshot.GetSize(), newSize);
                }
                tracker.ImagePath = SaveImage(snapshot,snapshot.GetSourceId(),tracker.Id);
            }
        }
        public abstract void OnDispose();
    }
}
