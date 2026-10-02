using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Extensions;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.InferenceModels.Sessions;
using NetLpr.Core.Values.Inference;
using Microsoft.ML.OnnxRuntime;

namespace NetLpr.InferenceOR
{
    public abstract class OnnxModelSession<T> : InferenceModel where T : unmanaged
    {
        private string _id = Guid.NewGuid().ToString();
        private int _disposed = 0;
        private readonly object _disposeLock = new object();
        public ModelConfig ModelConfig { get; private set; }
        public OnnxTensorPool<T> Pool { get; private set; }
        protected ILogger _logger;
        public OnnxModelSession(ILogger logger, OnnxModelFactory modelFactory, ModelConfig modelConfig) {
            _logger = logger;
            ModelConfig = modelConfig;
            Pool = new OnnxTensorPool<T>(logger, modelConfig, modelFactory);
        }
        private async Task<OnnxTensorInfo<T>> AcquireAsync()
        {
            return await Pool.AcquireAsync();
        }
        private void Release(OnnxTensorInfo<T> inferRequest)
        {
            Pool.Release(inferRequest);
        }
        private unsafe void ImageToTensor(BaseImage image, OnnxTensorInfo<T> tensor, RectangleF? roi = null)
        {
            image.PreprocessAndTransformTensor<T>(tensor.GetBufferPointer(), ModelConfig, roi);
        }
        public bool IsDisposed() => Interlocked.CompareExchange(ref _disposed, 0, 0) == 1;

        public ModelConfig GetModelConfig()
        {
            return ModelConfig;
        }


        private R Run<R>(OnnxTensorInfo<T> inferRequest, ModelConfig modelConfig,Size originalSize, Func<IDisposableReadOnlyCollection<DisposableNamedOnnxValue>, Size, R> func)
        {

            try
            {
                using var result = inferRequest.ModelSession.Run(inferRequest.Tensors);
                return func.Invoke(result,originalSize);

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"{GetType()} session  '{ModelConfig.ModelPath}' model loading error.", ex);
            }

        }
        public async Task<List<InferenceResult>> ProcessForMultiple(BaseImage image, RectangleF? roi = null)
        {
            OnnxTensorInfo<T>? inferRequest = null;
            try
            {
                inferRequest = await AcquireAsync();
                if (image == null || image.IsDisposedOrEmpty()) return new();
                ImageToTensor(image, inferRequest, roi);
                Size imageOriginalCoordinate = InferenceModel.PreprocessedImageSize(image, roi);

                var results = Run(inferRequest, ModelConfig, imageOriginalCoordinate,ResponseToDetectionResults);
                results.CalculateOriginalCoordinate(roi);
                return results;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{GetType().Name}:");
                return new();
            }
            finally
            {

                if (inferRequest != null)
                    Release(inferRequest);
            }
        }

        public async Task<InferenceResult?> ProcessForSingle(BaseImage image, RectangleF? roi = null)
        {
            OnnxTensorInfo<T>? inferRequest = null;
            try
            {
                inferRequest = await AcquireAsync();
                if (image == null || image.IsDisposedOrEmpty()) return new();
                ImageToTensor(image, inferRequest, roi);
                Size imageOriginalCoordinate = InferenceModel.PreprocessedImageSize(image, roi);
                var results = Run(inferRequest, ModelConfig, imageOriginalCoordinate, ResponseToDetectionResult);
                results.CalculateOriginalCoordinate(roi);
                return results;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{GetType().Name}:");
                return new();
            }
            finally
            {
                if (inferRequest != null)
                    Release(inferRequest);
            }
        }
        protected abstract InferenceResult? ResponseToDetectionResult(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> result, Size originalSize);
        protected abstract List<InferenceResult> ResponseToDetectionResults(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> result, Size originalSize);
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                lock (_disposeLock)
                {

                    try
                    {
                        Pool.Dispose();
                        GC.SuppressFinalize(this);
                        _logger.LogInformation($"{GetType().Name}: disposed");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{GetType().Name}: ");
                    }

                }
            }
        }
    }
}
