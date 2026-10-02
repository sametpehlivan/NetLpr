using System;
using System.Threading.Channels;
using NetLpr.Core;
using NetLpr.Core.Values.Inference;
using NetLpr.Core.Values.Preprocessing;
using OpenCvSharp;

namespace NetLpr.ImageProcessingCv
{
    public static class PreprocessingUtil
    {

        public static Mat? Crop(Mat image, System.Drawing.RectangleF? rectangle = null)
        {
            try
            {
                if (rectangle == null)
                    return null;
                int offset = 1;
                var rect = new Rect(
                    Math.Min(Math.Max((int)rectangle.Value.X + offset, 0), image.Width - 1),
                    Math.Min(Math.Max((int)rectangle.Value.Y + offset, 0), image.Height - 1),
                    Math.Max(1, Math.Min((int)Math.Min(rectangle.Value.Width, image.Width - (int)Math.Max(rectangle.Value.X, 0)) - offset,
                                        image.Width - Math.Min(Math.Max((int)rectangle.Value.X + offset, 0), image.Width - 1))),
                    Math.Max(1, Math.Min((int)Math.Min(rectangle.Value.Height, image.Height - (int)Math.Max(rectangle.Value.Y, 0)) - offset,
                                        image.Height - Math.Min(Math.Max((int)rectangle.Value.Y + offset, 0), image.Height - 1)))
                );
                return image[rect];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }
        }

        public static Image? Crop(Image wrapper, System.Drawing.RectangleF? rectangle = null, System.Drawing.Size? size = null)
        {
            try
            {
                if (wrapper == null || wrapper.IsDisposedOrEmpty())
                    return null;
                if (rectangle == null)
                {
                    rectangle = new System.Drawing.RectangleF(0, 0, wrapper.GetSize().Width, wrapper.GetSize().Height);
                }
                var cropped = Crop(wrapper.Mat, rectangle);
                if (cropped == null) return null;
                if (size != null && !size.Value.IsEmpty)
                {
                    Cv2.Resize(cropped, cropped, new Size(size.Value.Width, size.Value.Height), interpolation: InterpolationFlags.LinearExact);
                }

                return new Image(wrapper.GetFrameCount(),wrapper.GetSourceId(),cropped,wrapper.GetPixelFormat(),wrapper.GetStreamAnalysesInfo());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        private static void DrawRoundedCorners(Mat mat, System.Drawing.RectangleF rect, int cornerLength, int cornerRadius, Scalar color, int thickness)
        {
            Point topLeft = new Point((int)rect.X-5, (int)rect.Y-5);
            Point topRight = new Point((int)(rect.X+5 + rect.Width), (int)rect.Y-5);
            Point bottomLeft = new Point((int)rect.X-5, (int)(rect.Y + rect.Height+5));
            Point bottomRight = new Point((int)(rect.X+5 + rect.Width), (int)(rect.Y + rect.Height+5));

            // Sol üst köşe
            Cv2.Line(mat,
                new Point(topLeft.X + cornerRadius, topLeft.Y),
                new Point(topLeft.X + cornerLength, topLeft.Y),
                color, thickness);
            Cv2.Line(mat,
                new Point(topLeft.X, topLeft.Y + cornerRadius),
                new Point(topLeft.X, topLeft.Y + cornerLength),
                color, thickness);
            Cv2.Ellipse(mat,
                new Point(topLeft.X + cornerRadius, topLeft.Y + cornerRadius),
                new Size(cornerRadius, cornerRadius),
                180, 0, 90, color, thickness);

            // Sağ üst köşe
            Cv2.Line(mat,
                new Point(topRight.X - cornerRadius, topRight.Y),
                new Point(topRight.X - cornerLength, topRight.Y),
                color, thickness);
            Cv2.Line(mat,
                new Point(topRight.X, topRight.Y + cornerRadius),
                new Point(topRight.X, topRight.Y + cornerLength),
                color, thickness);
            Cv2.Ellipse(mat,
                new Point(topRight.X - cornerRadius, topRight.Y + cornerRadius),
                new Size(cornerRadius, cornerRadius),
                270, 0, 90, color, thickness);

            // Sol alt köşe
            Cv2.Line(mat,
                new Point(bottomLeft.X + cornerRadius, bottomLeft.Y),
                new Point(bottomLeft.X + cornerLength, bottomLeft.Y),
                color, thickness);
            Cv2.Line(mat,
                new Point(bottomLeft.X, bottomLeft.Y - cornerRadius),
                new Point(bottomLeft.X, bottomLeft.Y - cornerLength),
                color, thickness);
            Cv2.Ellipse(mat,
                new Point(bottomLeft.X + cornerRadius, bottomLeft.Y - cornerRadius),
                new Size(cornerRadius, cornerRadius),
                90, 0, 90, color, thickness);

            // Sağ alt köşe
            Cv2.Line(mat,
                new Point(bottomRight.X - cornerRadius, bottomRight.Y),
                new Point(bottomRight.X - cornerLength, bottomRight.Y),
                color, thickness);
            Cv2.Line(mat,
                new Point(bottomRight.X, bottomRight.Y - cornerRadius),
                new Point(bottomRight.X, bottomRight.Y - cornerLength),
                color, thickness);
            Cv2.Ellipse(mat,
                new Point(bottomRight.X - cornerRadius, bottomRight.Y - cornerRadius),
                new Size(cornerRadius, cornerRadius),
                0, 0, 90, color, thickness);
        }
        public static void DrawCorner(Mat mat, System.Drawing.RectangleF rect)
        {
            try
            {
                DrawRoundedCorners(mat, rect, (int) (20 * rect.Width / mat.Width), 6, Scalar.RandomColor(), 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public static void ResizeWithPaddingInPlace(ref Mat mat, Size targetSize, Scalar paddingColor = default, InterpolationFlags interpolation = InterpolationFlags.Linear)
        {
            if (mat == null || mat.Empty())
                return;

            if (paddingColor == default)
                paddingColor = Scalar.Black;

            Mat tempResized = null;
            Mat paddedMat = null;

            try
            {

                double scaleX = (double)targetSize.Width / mat.Width;
                double scaleY = (double)targetSize.Height / mat.Height;
                double scale = Math.Min(scaleX, scaleY);

                int newWidth = (int)(mat.Width * scale);
                int newHeight = (int)(mat.Height * scale);

                tempResized = new Mat();
                Cv2.Resize(mat, tempResized, new Size(newWidth, newHeight), 0, 0, interpolation);

                paddedMat = new Mat(targetSize, tempResized.Type(), paddingColor);
                int offsetX = (targetSize.Width - newWidth) / 2;
                int offsetY = (targetSize.Height - newHeight) / 2;
                Rect roi = new Rect(offsetX, offsetY, newWidth, newHeight);

                tempResized.CopyTo(paddedMat[roi]);

                mat.Dispose();
                mat = paddedMat;
                paddedMat = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

            }
            finally
            {
                tempResized?.Dispose();

            }
        }

        private unsafe static void PreprocessImage(
            ref Mat workingMat,
            PixelFormat srcPixelFormat,
            ModelConfig config
        )
        {
            if (workingMat == null || workingMat.Empty())
                throw new ArgumentException("Mat boş olamaz");
            var dims = config.ImageSize;
            int width = dims.Width, height = dims.Height;
            var targetSize = new Size(width, height);
            try
            {

                if (config.ScalingPolicy.Equals(ScalingPolicy.RespectAspectRatio))
                {
                    Scalar padColor = config.PaddingColor.ToScalar();
                    ResizeWithPaddingInPlace(ref workingMat, targetSize, padColor);
                }
                else
                {
                    Mat tempResize = new Mat();
                    Cv2.Resize(workingMat, tempResize, targetSize);
                    workingMat.Dispose();
                    workingMat = tempResize;
                }



                if (config.Normalize != Normalize.NONE)
                {
                    workingMat.ConvertTo(workingMat, MatType.CV_32FC3, 1.0 / 255.0);
                    if(config.Normalize != Normalize.MIN_MAX)
                    {
                        Mean mean = config.Normalize.Mean;
                        Std std = config.Normalize.Std;
                        var meanScalar = new Scalar(mean.R, mean.G, mean.B);
                        var stdScalar = new Scalar(1.0f / std.R, 1.0f / std.G, 1.0f / std.B);
                        Cv2.Subtract(workingMat, meanScalar, workingMat);
                        Cv2.Multiply(workingMat, stdScalar, workingMat);
                    }

                }
                var converCode = srcPixelFormat.PixelFormatToColorConversions(config.PixelFormat);
                if (converCode != null)
                {
                    Cv2.CvtColor(workingMat, workingMat, converCode.Value);
                }
                if (config.ImageTensorInfo.ImageTensorLayoutType == ImageTensorLayoutType.BCHW)
                {
                    int channelsCount = workingMat.Channels();
                    int rows = workingMat.Rows;
                    int cols = workingMat.Cols;

                    Mat[] channels = Cv2.Split(workingMat);

                    Mat bchwMat = new Mat(channelsCount * rows, cols, channels[0].Type());

                    for (int i = 0; i < channelsCount; i++)
                    {
                        using Mat roiBchw = new Mat(bchwMat, new Rect(0, i * rows, cols, rows));
                        channels[i].CopyTo(roiBchw);
                        channels[i].Dispose();
                    }

                    workingMat.Dispose();
                    workingMat = bchwMat;
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                workingMat?.Release();
                throw;
            }
        }

        public static Mat PreprocessImage(
            PixelFormat srcPixelFormat,
            Mat originalMat,
            ModelConfig config,
            System.Drawing.RectangleF? roi = null
        )
        {
            Mat workingMat;
            if (roi != null && !System.Drawing.RectangleF.Empty.Equals(roi))
            {
                var mat = Crop(originalMat, roi);
                if (mat == null || mat.Empty() || mat.IsDisposed)
                    throw new Exception("Image Dispose Edilmiş");
                workingMat = mat;
            }
            else
            {
                workingMat = new Mat();
                originalMat.CopyTo(workingMat);
            }
       
            PreprocessImage(ref workingMat, srcPixelFormat,config);
            return workingMat;
        }
    
    }
}
