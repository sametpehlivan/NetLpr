using System.Drawing;
using NetLpr.Core.Models;
using NetLpr.Core.Values.Inference;

namespace NetLpr.Core.Services.InferenceModels.Sessions
{
    public interface InferenceModel : IDisposable
    {
        public Task<List<InferenceResult>> ProcessForMultiple(BaseImage image, System.Drawing.RectangleF? roi = null);
        public Task<InferenceResult?> ProcessForSingle(BaseImage image, System.Drawing.RectangleF? roi = null);
        public ModelConfig GetModelConfig();
        public bool IsDisposed();
        public static Size PreprocessedImageSize(BaseImage image, RectangleF? roi)
        {
            Size originalSize = image.GetSize();
            if (roi != null)
                originalSize = new Size((int)roi.Value.Width, (int)roi.Value.Height);
            return originalSize;
        }

    }
}
