using System.Drawing;
using System.Reflection.Emit;
using System.Xml.Linq;
using NetLpr.Core.Values.Inference;
using NetLpr.Core.Values.Preprocessing;

namespace NetLpr.Core.Models
{
    public interface IDecodedFrame 
    {
        public string GetSourceId();
        public ulong GetFrameCount();
        public Size GetSize();
        public void TransformTo(nint buffer, int bufferStride, PixelFormat pixelFormat);
        public StreamAnalysesInfo GetStreamAnalysesInfo();
     
    }
    public abstract class BaseImage : IDecodedFrame, IDisposable
    {
        public abstract PixelFormat GetPixelFormat();
        public abstract Size GetSize();
        public abstract bool IsDisposedOrEmpty();
        public abstract BaseImage? CropAndResizeAndDrawDetectionResults(Size size, List<InferenceResult?> detectionResults, RectangleF? roi = null);
        public unsafe abstract void PreprocessAndTransformTensor<T>(void* pointer, ModelConfig modelInformation, RectangleF? roi) where T : unmanaged;
        public abstract void TransformTo(nint buffer, int bufferStride, PixelFormat pixelFormat);
        public abstract void Dispose();
        public abstract StreamAnalysesInfo GetStreamAnalysesInfo();
        public abstract ulong GetFrameCount();
        public abstract string GetSourceId();
        public abstract bool Save(string path);


    }
}
