using System.Drawing;
using NetLpr.Core.Datas;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.Inference.Pipelines;
using NetLpr.Core.Services.Sources;
using NetLpr.Core.Services.Syncronizations;
using NetLpr.Core.Services.Tracking;
using NetLpr.Core.Values.Inference;
using NetLpr.Core.Values.Tracking;

namespace NetLpr.Core.Services.Node
{
    public class RtspNode : IDisposable
    {
        private object _syncLock = new object();
        private ILogger _logger;
        private volatile bool _disposed;
        private RtspSource _sourceService;
        private ITrackingService? _trackingService;
        private SyncronizationService? _synchronizationService;
        private IInferencePipeline? _inferencePipeLine;
        
        public RtspNode(
            ILogger logger,
            RtspSource sourceService,
            SyncronizationService synchronizationService,
            ITrackingService? trackingService,
            IInferencePipeline? vehicleDetectionPipeline
        )
        { 
            _logger = logger;
            _synchronizationService = synchronizationService;
            _inferencePipeLine = vehicleDetectionPipeline;
            _sourceService = sourceService;
            _trackingService = trackingService;
         
            Initiate();
        }
        private void Initiate()
        {
            _sourceService.SubscribeForInferenceInput((s, f) =>
            {
                if (_synchronizationService == null && _inferencePipeLine == null)
                {
                    f.Dispose();
                    return;
                }
                    
                if (_synchronizationService != null)
                {
                    _synchronizationService.OnFrameReading(s, f);
                    if (_inferencePipeLine != null)
                        _inferencePipeLine.ProcessAsync(f);
                    else
                        f.MarkProcessed();
                }
            });
            _synchronizationService?.Subscribe((s, f) => {
                if (_trackingService == null) f.Dispose();
                else _trackingService.OnFrameReading(s,f);
            });
        }
        public RtspSourceInfo GetRtspSourceInfo()
        {
           return _sourceService.RtspSourceInfo;
        }
        public void SubscribeForMessages(EventHandler<string> eventHandler)
        {
            _sourceService.SubscribeForMessages(eventHandler);
        }
        public void UnsubscribeForMessages(EventHandler<string> eventHandler)
        {
            _sourceService.UnsubscribeForMessages(eventHandler);
        }
        public void SubscribeForDecodedFrame(EventHandler<IDecodedFrame> eventHandler)
        {
            _sourceService.SubscribeForDecodedFrame(eventHandler);
        }
        public void UnsubscribeForDecodedFrame(EventHandler<IDecodedFrame> eventHandler)
        {
            _sourceService.UnsubscribeForDecodedFrame(eventHandler);
        }
        public void UpdateRtspSourceInfo(RtspSourceInfo sourceInfo)
        {
            _sourceService.UpdateRtspSourceInfo(sourceInfo);
        }
        public void SubscribeTrackingCompleted(EventHandler<TrackedInfo> eventHandler)
        {
            _trackingService?.Subscribe(eventHandler);
        }
        public void UnsubscribeTrackingCompleted(EventHandler<TrackedInfo> eventHandler)
        {
            _trackingService?.Unsubscribe(eventHandler);
        }
      

        public bool IsDisposed()
        {
            return _disposed;
        }
        public async Task StartAsync()
        {
            try
            {
                await _sourceService.StartAsync();
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"{GetType().Name}: source starting error!");
            }
        }
   
        public void Dispose()
        {
            lock (_syncLock)
            {
                if (IsDisposed()) return;
                try
                {
                    _sourceService.Dispose();
                  
                    _inferencePipeLine?.Dispose();
                    _trackingService?.Dispose();
                    _synchronizationService?.Dispose();
                    _disposed = true;
                    GC.SuppressFinalize(this);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{GetType().Name}: disposing error!");
                }
            }
        }


    }
}
