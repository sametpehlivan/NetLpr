using System;
using NetLpr.Core.Values.Preprocessing;
using OpenCvSharp;

namespace NetLpr.ImageProcessingCv
{
    public static class TransformExtension
    {
        public static Rect ToSafeRect2d(this System.Drawing.RectangleF rectF, int imageWidth, int imageHeight)
        {
            float x1 = Math.Max(0, rectF.X);
            float y1 = Math.Max(0, rectF.Y);

            float x2 = Math.Min(imageWidth, rectF.Right);
            float y2 = Math.Min(imageHeight, rectF.Bottom);

            float width = x2 - x1;
            float height = y2 - y1;

            if (width <= 0 || height <= 0)
                return new Rect();

            return new Rect((int)x1, (int)y1, (int)width, (int)height);
        }

        private static void ConvertColor(Mat source, Mat destination, PixelFormat srcFormat,PixelFormat dstFormat)
        {
            ColorConversionCodes? colorConversionCode = srcFormat.PixelFormatToColorConversions(dstFormat);
            if(colorConversionCode == null)
            {
                if(!ReferenceEquals(source,destination))
                    source.CopyTo(destination);
            }
            else
            {
                Cv2.CvtColor(source, destination, colorConversionCode.Value);

            }
        }
        public static void TransformTo(Mat mat, PixelFormat srcFormat,nint buffer, int bufferStride, PixelFormat pixelFormat)
        {
            var image = new Mat();
            var roi = new System.Drawing.RectangleF(0, 0, mat.Width, mat.Height);
            var rect = roi.ToSafeRect2d(mat.Width, mat.Height);
            image = new Mat(mat, rect);

            ConvertColor(image, image, srcFormat, pixelFormat);
            CopyToBuffer(image, buffer, bufferStride);
            image.Dispose();
        }

        private static unsafe void CopyToBuffer(Mat mat, IntPtr buffer, int bufferStride)
        {
            var matStride = (int)mat.Step();
            var height = mat.Height;
            var bytesPerRow = mat.Width * mat.Channels();

            byte* bufferPtr = (byte*)buffer.ToPointer();
            byte* matPtr = (byte*)mat.Data.ToPointer();
            if (matStride == bufferStride && matStride == bytesPerRow)
            {
                Buffer.MemoryCopy(matPtr, bufferPtr, bufferStride * height, matStride * height);
                return;
            }

            for (int row = 0; row < height; row++)
            {
                Buffer.MemoryCopy(
                    matPtr + (row * matStride),
                    bufferPtr + (row * bufferStride),
                    bytesPerRow,
                    bytesPerRow
                );
            }
        }
    }
}
