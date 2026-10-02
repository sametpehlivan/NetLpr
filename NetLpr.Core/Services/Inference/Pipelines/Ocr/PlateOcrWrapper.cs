using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.InferenceModels.Sessions;
using System.Drawing;
using System.Threading;

namespace NetLpr.Core.Services.Inference.Pipelines.Ocr
{
    public class PlateOcrWrapper : IDisposable
    {
        private readonly InferenceModel _plateOcr;
        private readonly ILogger _logger;
        private bool _disposed;

        public PlateOcrWrapper(ILogger logger, InferenceModel plateOcr)
        {
            _plateOcr = plateOcr ?? throw new ArgumentNullException(nameof(plateOcr));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        private bool ValidateInputs(BaseImage image)
        {
            if (_disposed)
            {
                _logger.LogWarning($"{GetType().Name} disposed!");
                return false;
            }
            if (_plateOcr.IsDisposed())
            {
                _logger.LogWarning($"{_plateOcr.GetType().Name} disposed!");
                return false;
            }
            if (image == null)
            {
                _logger.LogWarning($"{nameof(image)} frame null");
                return false;
            }
            if (image.IsDisposedOrEmpty())
            {
                _logger.LogWarning($"{image.GetType().Name} frame disposed");
                return false;
            }
            return true;
        }
        public async Task<InferenceResult> ProcessAsync(BaseImage image,RectangleF roi)
        {

            try
            {
                if (!ValidateInputs(image))
                    return InferenceResult.Empty;
                return await _plateOcr.ProcessForSingle(image, roi) ?? InferenceResult.Empty;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{GetType().Name} error!");
                return InferenceResult.Empty;
            }
           
        }

        public void Dispose()
        {
            try
            {
                if (_disposed)
                    return;
                _plateOcr.Dispose();
                _disposed = true;
                GC.SuppressFinalize(this);
                _logger.LogInformation($"{GetType().Name}: disposing successful!");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name}: disposing fail!");
            }

        
        }
    }
}
