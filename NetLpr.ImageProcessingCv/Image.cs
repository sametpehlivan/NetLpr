using System.Drawing;
using NetLpr.Core;
using NetLpr.Core.Extensions;
using NetLpr.Core.Models;
using NetLpr.Core.Values.Inference;
using NetLpr.Core.Values.Preprocessing;
using OpenCvSharp;

namespace NetLpr.ImageProcessingCv
{
    public class Image : BaseImage
    {
        private object _lock = new object();
        private volatile bool _disposed = false;
        private PixelFormat _pixelFormat;
        internal Mat Mat;
        private StreamAnalysesInfo _streamAnalysesInfo;
        private ulong _frameCount;
        private string _sourceId;
        public Image(ulong frameCount,string sourceId,Mat mat,PixelFormat format,StreamAnalysesInfo streamAnalysesInfo)
        {
            Mat = mat;
            _sourceId = sourceId;
            _pixelFormat = format;
            _streamAnalysesInfo = streamAnalysesInfo;
            _frameCount = frameCount;
        }
        public override System.Drawing.Size GetSize()
        {
            return IsDisposedOrEmpty() ? new System.Drawing.Size(0, 0) : new System.Drawing.Size(Mat.Width, Mat.Height);
        }
        public override bool IsDisposedOrEmpty()
        {
            return Mat == null || Mat.IsDisposed || _disposed;
        }

        public override BaseImage? CropAndResizeAndDrawDetectionResults(System.Drawing.Size size, List<InferenceResult?> detectionResults, System.Drawing.RectangleF? roi = null)
        {
            if (IsDisposedOrEmpty()) return null;
            if (detectionResults != null && detectionResults.Count() > 0)
                detectionResults.ForEach(det =>
                {
                    if (det != null)
                        PreprocessingUtil.DrawCorner(this.Mat, det.ToRectangleF());
                });
            var cropAndResized = PreprocessingUtil.Crop(this, roi, size);
            if (cropAndResized == null || cropAndResized.IsDisposedOrEmpty()) return null;
           
            return cropAndResized;
        }

        public override void TransformTo(nint buffer, int bufferStride, PixelFormat pixelFormat)
        {
            try
            {
                TransformExtension.TransformTo(Mat, GetPixelFormat(),buffer, bufferStride, pixelFormat);
            }
            catch    
            {
                return;
            }

        }
        public override void Dispose()
        {
            if (!_disposed)
         
            {
                lock (_lock)
                {
                    if (!_disposed)
                    {
                        try
                        {
                            Mat.Dispose();
                            GC.SuppressFinalize(this);
                        }
                        catch
                        {
                           return ;
                        }
                    }
                }
            }
        }

        public override unsafe void PreprocessAndTransformTensor<T>(void* pointer, ModelConfig modelConfig,System.Drawing.RectangleF? roi) 
        {
            if (!IsDisposedOrEmpty())
                PreprocessToTensorPointer.PreprocessAndTransformTensor<T>(this, pointer, modelConfig, roi);

        }

        public override PixelFormat GetPixelFormat()
        {
            return _pixelFormat;
        }

        public override StreamAnalysesInfo GetStreamAnalysesInfo()
        {
            return _streamAnalysesInfo;
        }

        public override ulong GetFrameCount()
        {
            return _frameCount;
        }

        public override string GetSourceId()
        {
            return _sourceId;
        }

        public override bool Save(string path)
        {
            if(IsDisposedOrEmpty())
                return false;
            try
            {
                var res = Mat.SaveImage(path);
                return res;
            }
            catch
            {
                return false;
            }
        }


    }
}
