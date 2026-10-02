using System.Collections.Concurrent;
using System.Drawing;
using NetLpr.Core;
using NetLpr.Core.Extensions;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.InferenceModels.Sessions;
using NetLpr.Core.Values.Inference;
using OpenVinoSharp;

namespace NetLpr.InferenceOV
{
    public abstract class OpenVinoModel<T> : InferenceModel where T : unmanaged
    {
        private int _disposed = 0;
        private readonly object _disposeLock = new object();
        public ModelConfig ModelConfig { get; private set; }
        public OpenVinoInferenceRequestPool Pool { get; private set; }
        private ILogger _logger;
        public bool IsDisposed() => Interlocked.CompareExchange(ref _disposed, 0, 0) == 1;
        public OpenVinoModel(ILogger logger,OpenVinoInferencePoolFactory poolFactory, ModelConfig modelConfig)
        {
            modelConfig.Validate();
            _logger = logger;
            ModelConfig = modelConfig;
            Pool = poolFactory.GetModel(ModelConfig);
        }
        private async Task<InferRequest> AcquireAsync() {
            return await Pool.AcquireAsync();    
        }
        private void Release(InferRequest inferRequest)
        {
            Pool.Release(inferRequest);
        }

        private R Run<R>(InferRequest inferRequest, Size originalSize ,Func<InferRequest, Size,R> func)
        {
            try
            {
                inferRequest.infer();
                return func.Invoke(inferRequest, originalSize);
                
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"{GetType()} session  '{ModelConfig.ModelPath}' model loading error.", ex);
            }
            
        }
        private unsafe void ImageToTensor(BaseImage image, Tensor tensor, RectangleF? roi = null)
        {
            image.PreprocessAndTransformTensor<T>(tensor.data().ToPointer(), ModelConfig, roi);
        }
        protected abstract InferenceResult? ResponseToDetectionResult(InferRequest result, Size originalSize);
        protected abstract List<InferenceResult> ResponseToDetectionResults(InferRequest result, Size originalSize);
    
        public async Task<List<InferenceResult>> ProcessForMultiple(BaseImage image,RectangleF? roi)
        {
            InferRequest? inferRequest = null;
            try
            {
                inferRequest = await AcquireAsync();
                if (image == null || image.IsDisposedOrEmpty()) return new();
                var tensorName = ModelConfig.ImageTensorName;
                var inputTensor = inferRequest.get_tensor(tensorName);
                ImageToTensor(image, inputTensor, roi);
                Size imageOriginalCoordinate = InferenceModel.PreprocessedImageSize(image, roi);
                var results = Run(inferRequest, imageOriginalCoordinate, ResponseToDetectionResults);
                results.CalculateOriginalCoordinate(roi);
                return results;
            }
            catch(Exception e)
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
        
        public async Task<InferenceResult?> ProcessForSingle(BaseImage image,RectangleF? roi = null)
        {
            InferRequest? inferRequest = null;
            try
            {
                inferRequest = await AcquireAsync();
                if (image == null || image.IsDisposedOrEmpty()) return new();
                var inputTensor = inferRequest.get_tensor(ModelConfig.ImageTensorName);
                ImageToTensor(image, inputTensor, roi);
                Size imageOriginalCoordinate = InferenceModel.PreprocessedImageSize(image, roi);
                var result = Run(inferRequest, imageOriginalCoordinate, ResponseToDetectionResult);
                result.CalculateOriginalCoordinate(roi);
                return result;
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"{GetType().Name}:");
                return null;
            }
            finally
            {
                if(inferRequest != null)
                    Release(inferRequest);
            }
        }
   
        public ModelConfig GetModelConfig()
        {
            return ModelConfig;
        }
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