using System.Collections.Concurrent;
using NetLpr.Core.Log;
using OpenVinoSharp;

namespace NetLpr.InferenceOV
{
    public class OpenVinoInferenceRequestPool : IDisposable
    {
        private readonly ConcurrentQueue<InferRequest> _pool = new();
        private readonly SemaphoreSlim _semaphore;
        private int _disposed = 0;
        private ILogger _logger;
        public CompiledModel _compiledModel { get; }

        public OpenVinoInferenceRequestPool(ILogger logger,CompiledModel compiledModel)
        {
            if (compiledModel == null)
                throw new ArgumentNullException(nameof(compiledModel));

            int poolSize = 4;
            _logger = logger;
            _semaphore = new SemaphoreSlim(poolSize, poolSize);
            _compiledModel = compiledModel;


            for (int i = 0; i < poolSize; i++)
            {
                var req = compiledModel.create_infer_request();
                req.infer();
                _pool.Enqueue(req);
            }
        }
        public async Task<InferRequest> AcquireAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();

            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            if (_pool.TryDequeue(out var request))
            {
                return request;
            }
               

            throw new InvalidOperationException("");
        }

        public void Release(InferRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (IsDisposed())
                throw new ObjectDisposedException(nameof(OpenVinoInferenceRequestPool));

            _pool.Enqueue(request);
            _semaphore.Release();
        }
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            _semaphore?.Dispose();
            while (_pool.TryDequeue(out var req))
            {
                try
                {
                    req.Dispose();

                }
                catch(Exception e)
                {
                    _logger.LogError(e,$"{GetType().Name}");
                }
            }
            _compiledModel.Dispose();
            _logger.LogInformation($"{GetType().Name} disposed");
            GC.SuppressFinalize(this);
        }


        private bool IsDisposed() => _disposed == 1;

        private void ThrowIfDisposed()
        {
            if (IsDisposed())
                throw new ObjectDisposedException(nameof(OpenVinoInferenceRequestPool));
        }
    }
}
