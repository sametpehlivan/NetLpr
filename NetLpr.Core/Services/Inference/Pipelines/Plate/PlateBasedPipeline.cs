using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Datas;
using NetLpr.Core.Extensions;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.Inference.Pipelines.Ocr;
using NetLpr.Core.Services.InferenceModels.Sessions;
using static System.Net.Mime.MediaTypeNames;

namespace NetLpr.Core.Services.Inference.Pipelines.Plate
{
    public class PlateBasedPipeline : IInferencePipeline
    {
        private readonly ILogger _logger;
        private InferenceModel _plateDetection;
        private PlateOcrWrapper _plateOcrWrapper;
        private volatile bool _disposed = false;
        private SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(4,4);
        public PlateBasedPipeline(ILogger logger,InferenceModel plateDetection, PlateOcrWrapper plateOcrWrapper)
        {
            _logger = logger;
            _plateDetection = plateDetection;
            _plateOcrWrapper = plateOcrWrapper;
        }
        private bool ValidateInputs(InferencedInput input)
        {

            if (_disposed)
            {
                _logger.LogWarning($"{GetType().Name} disposed!");
                return false;
            }
            if (_plateDetection.IsDisposed())
            {
                _logger.LogWarning($"{_plateDetection.GetType().Name} disposed!");
                return false;
            }

            var image = input.Image;
            if (image!.IsDisposedOrEmpty())
            {
                _logger.LogWarning($"{image.GetType().Name} frame disposed!");
                return false;
            }
            return true;
        }
        public async Task ProcessAsync(InferencedInput input)
        {
            try
            {
                if (!ValidateInputs(input))
                {
                    input.MarkProcessed();
                    return;
                }
                var image = input.Image!;
                List<InferenceResult> plates;
                if (input.StreamAnalysesInfo.RoiIsFullOrEmpty())
                    plates = await _plateDetection.ProcessForMultiple(image);
                else
                {
                    var rectanglef = input.StreamAnalysesInfo.AnalysesRoi;
                    var imageSize = image.GetSize();

                    var rectangle = new Rectangle(
                        (int)(rectanglef.X * imageSize.Width),
                        (int)(rectanglef.Y * imageSize.Height),
                        (int)(rectanglef.Width * imageSize.Width),
                        (int)(rectanglef.Height * imageSize.Height)
                    );
                    plates = await _plateDetection.ProcessForMultiple(image, rectangle);

                }
                var tasks = plates.Select(async plateDet =>
                {
                    var ocrResult = await _plateOcrWrapper.ProcessAsync(image, plateDet.ToRectangleF());
                    plateDet.AddInformation(InferenceResult.INFORMATION_PLATE_OCR_INFERENCE_KEY, ocrResult);
                });
                await Task.WhenAll(tasks);
                input.DetectionResults.AddRange(plates);
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name}: Görüntü işlenirken bir hata oluştu.", ex);
            }
            finally
            {
                input.MarkProcessed();
            }
        }
        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                if (_disposed)
                    return;

                _plateDetection.Dispose();
                _plateOcrWrapper.Dispose();
                _disposed = true;
                GC.SuppressFinalize(this);
                _logger.LogInformation($"{GetType().Name} disposing successful!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name} disposing error!");
            }

        }
    }
}
