using System.Drawing;
using System.Threading;
using NetLpr.Core.Datas;
using NetLpr.Core.Extensions;
using NetLpr.Core.Localization;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.Syncronizations;

namespace NetLpr.Core.Services.Sources
{
    public abstract class RtspSource : IDisposable
    {

        private volatile bool _disposed;
        private object _syncLock = new object();
        private EventHandler<InferencedInput> _inputHandler = (_, _) => { };

        protected ILogger _logger;
     
        
        protected SynchronizationServiceContext _synchronizationContext;
        protected RtspSourceContext _sourceServiceContext;
        protected ILocalizationService _localizationService;

        protected EventHandler<IDecodedFrame> _decodedFrameHandler;
        private Func<IDecodedFrame, BaseImage> _converterFunc;
        private EventHandler<string> _sourceMessageHandler;
        public RtspSourceInfo RtspSourceInfo { get;}


        public RtspSource(
            RtspSourceInfo rtspSourceInfo,
            ILogger logger,
            ILocalizationService localizationService,
            RtspSourceContext sourceServiceContext, 
            SynchronizationServiceContext synchronizationContext,
            Func<IDecodedFrame,BaseImage> converterFunc
        )
        {
             RtspSourceInfo = rtspSourceInfo;
            _logger = logger;
            _localizationService = localizationService;
          
            _converterFunc = converterFunc;
            _synchronizationContext = synchronizationContext;
            _sourceServiceContext = sourceServiceContext;
            _sourceMessageHandler = (s, message) => {};
            _decodedFrameHandler = (s, input) =>
            {
                _sourceServiceContext.Enqueue(RtspSourceInfo.Id, input);
                
            };
            _sourceServiceContext.Register(RtspSourceInfo.Id, Invoke);


        }
        private void Invoke(IDecodedFrame input)
        {

            if(_inputHandler.GetInvocationList().Count() != 1 && input.GetStreamAnalysesInfo().IsAnalysesOpen)
            {
                try
                {
                    var inferencedInput = new InferencedInput(input.GetSourceId(),input.GetStreamAnalysesInfo())
                    {
                        Image = _converterFunc.Invoke(input)
                    };
                    _inputHandler.Invoke(this, inferencedInput);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Frame not converted to image");
                    return;
                }
            }
        }
    

        public void SubscribeForInferenceInput(EventHandler<InferencedInput> eventHandler)
        {
            if (eventHandler == null)
                return;
            lock (_syncLock)
            {
                if (IsDisposed())
                {
                    _logger.LogError($"{nameof(SyncronizationService)} object disposed");
                    return;
                }

                _inputHandler += eventHandler;
            }
        }

        public void UnsubscribeForInferenceInput(EventHandler<InferencedInput> eventHandler)
        {
            if (eventHandler == null)
                return;

            lock (_syncLock)
            {
                if (IsDisposed())
                {
                    _logger.LogError($"{nameof(SyncronizationService)} object disposed");
                    return;
                }
                _inputHandler -= eventHandler;
            }
        }

        public void SubscribeForDecodedFrame(EventHandler<IDecodedFrame> eventHandler)
        {
            if (eventHandler == null)
                return;
            lock (_syncLock)
            {
                if (IsDisposed())
                {
                    _logger.LogError($"{nameof(SyncronizationService)} object disposed");
                    return;
                }

                _decodedFrameHandler += eventHandler;
            }
        }

        public void UnsubscribeForDecodedFrame(EventHandler<IDecodedFrame> eventHandler)
        {
            if (eventHandler == null)
                return;
            lock (_syncLock)
            {
                if (IsDisposed())
                {
                    _logger.LogError($"{nameof(SyncronizationService)} object disposed");
                    return;
                }

                _decodedFrameHandler -= eventHandler;
            }
        }
        public void SubscribeForMessages(EventHandler<string> eventHandler)
        {
            if (eventHandler == null)
                return;
            lock (_syncLock)
            {
                if (IsDisposed())
                {
                    _logger.LogError($"{nameof(SyncronizationService)} object disposed");
                    return;
                }

                _sourceMessageHandler += eventHandler;
            }
        }
        public void UnsubscribeForMessages(EventHandler<string> eventHandler)
        {
            if (eventHandler == null)
                return;
            lock (_syncLock)
            {
                if (IsDisposed())
                {
                    _logger.LogError($"{nameof(SyncronizationService)} object disposed");
                    return;
                }

                _sourceMessageHandler -= eventHandler;
            }
        }
        public bool IsDisposed()
        {
            return _disposed;
        }

        public void Dispose()
        {
            lock (_syncLock)
            {
                if (IsDisposed()) return;

                try
                {
                    _synchronizationContext.Unregister(RtspSourceInfo.Id);
                    BeforeOnDispose();
                    _inputHandler = (_, _) => { };
                    _decodedFrameHandler = (_, _) => { };
                    _disposed = true;
                    _logger.LogInformation($"{GetType().Name} disposed!");
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex,$"{this.GetType().Name}: on dispose error !");
                }
        

            }

        }
        public abstract Task StartAsync();

        public abstract void BeforeOnDispose();

        public void SetStatus(RtspSourceStatus sourceStatus, params object[] args)
        {
            RtspSourceInfo.RtspSourceStatus = sourceStatus;
            
            if(sourceStatus == RtspSourceStatus.INITIALIZED)
                MessageReceive(this, new LocalizationMessage("rtspSource.status.initialized", args));
            if (sourceStatus == RtspSourceStatus.CONNECTING)
                MessageReceive(this, new LocalizationMessage("rtspSource.status.connecting", args));
            if (sourceStatus == RtspSourceStatus.CONNECTED)
                MessageReceive(this, new LocalizationMessage("rtspSource.status.connected", args));
            if (sourceStatus == RtspSourceStatus.STARTING)
                MessageReceive(this, new LocalizationMessage("rtspSource.status.starting", args));
            if (sourceStatus == RtspSourceStatus.DISCONNECTED)
                MessageReceive(this, new LocalizationMessage("rtspSource.status.disconnected", args));
            if (sourceStatus == RtspSourceStatus.CLOSING)
                MessageReceive(this, new LocalizationMessage("rtspSource.status.closing", args));
        }
        public void MessageReceive(object? sender, LocalizationMessage message)
        {
            _logger.LogLocalizationMessage(_localizationService, message);
            if (_sourceMessageHandler != null)
            {
                var response = _localizationService.CheckLogMessage(message.Message, message.Args);
                _sourceMessageHandler(sender, response.message);
            }
        }
        public abstract void UpdateRtspSourceInfo(RtspSourceInfo sourceInfo);
    }
}
