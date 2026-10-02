using NetLpr.Core.Datas;
using NetLpr.Core.Log;

namespace NetLpr.Core.Services.Syncronizations
{
    public class SyncronizationService : IDisposable
    {
        private string _serviceId;
        private readonly object _syncLock = new object();
        private volatile bool _disposed;
        private EventHandler<InferencedInput> _eventHandler = (_,_) => { };
        private ILogger _logger;
        private SynchronizationServiceContext _synchronizationServiceContext;
        public SyncronizationService(ILogger logger, SynchronizationServiceContext synchronizationServiceContext)
        {
            _serviceId = $"SyncronizationService-{Guid.NewGuid().ToString()}";
            _disposed = false;
            _logger = logger;
            _synchronizationServiceContext = synchronizationServiceContext;
            _synchronizationServiceContext.Register(_serviceId, Invoke);
        }
        private void Invoke(InferencedInput input)
        {
            if (_eventHandler.GetInvocationList().Count() == 1)
            {
                input.Dispose();
            }
            else 
                _eventHandler.Invoke(this, input);
        }
        public bool IsDisposed()
        {
            return _disposed;
        }

        public void OnFrameReading(object? sender,InferencedInput eventData)
        {

            if (!IsDisposed())
            {
                lock (_syncLock)
                {
                    if (!IsDisposed())
                        _synchronizationServiceContext.Enqueue(_serviceId,eventData);
                }
            }

        }

        public void Subscribe(EventHandler<InferencedInput> eventHandler)
        {
            if (eventHandler == null)
                return;

            lock (_syncLock)
            {
                if (IsDisposed())
                {
                    _logger.LogError($"{nameof(SyncronizationService)} disposed");
                    return;
                }

                _eventHandler += eventHandler;
            }
        }

        public void Unsubscribe(EventHandler<InferencedInput> eventHandler)
        {
            if (eventHandler == null)
                return;

            lock (_syncLock)
            {
                if (IsDisposed())
                {
                    _logger.LogError($"{nameof(SyncronizationService)} disposed");
                    return;
                }
                _eventHandler -= eventHandler;
            }
        }

        public void Dispose()
        {
            lock (_syncLock)
            {
                if (IsDisposed())
                    return;
                try
                {
                    _synchronizationServiceContext.Unregister(_serviceId);
                    _eventHandler = (_, _) => { };
                    _disposed = true;
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex, $"{GetType().Name}");
                }
            
            
            }
        }
    }
}