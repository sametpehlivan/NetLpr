using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Log;
using NetLpr.Core.Values.Inference;

namespace NetLpr.InferenceOR
{
    public class OnnxTensorPool<T> where T : unmanaged
    {
        private readonly ConcurrentQueue<OnnxTensorInfo<T>> _pool = new();
        private readonly SemaphoreSlim _semaphore;
        private int _disposed = 0;
        private ILogger _logger;

        public OnnxTensorPool(ILogger logger,ModelConfig modelConfig,OnnxModelFactory onnxModelFactory)
        {


            int poolSize = 2;
            _logger = logger;
            _semaphore = new SemaphoreSlim(poolSize, poolSize);


            for (int i = 0; i < poolSize; i++)
            {
                var model = onnxModelFactory.CreateSingleThreadedSession(modelConfig);
                var req = new OnnxTensorInfo<T>(modelConfig, model);
                _pool.Enqueue(req);
            }
        }
        public async Task<OnnxTensorInfo<T>> AcquireAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();

            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            if (_pool.TryDequeue(out var request))
            {
                return request;
            }


            throw new InvalidOperationException("");
        }

        public void Release(OnnxTensorInfo<T> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (IsDisposed())
                throw new ObjectDisposedException(nameof(OnnxTensorPool<T>));

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
                catch (Exception e)
                {
                    _logger.LogError(e, $"{GetType().Name}");
                }
            }
            _logger.LogInformation($"{GetType().Name} disposed");
            GC.SuppressFinalize(this);
        }


        private bool IsDisposed() => _disposed == 1;

        private void ThrowIfDisposed()
        {
            if (IsDisposed())
                throw new ObjectDisposedException(nameof(OnnxTensorPool<T>));
        }
    }
}
