using System;
using System.Drawing;
using Avalonia;
using Avalonia.Logging;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NetLpr.Core;
using NetLpr.Core.Models;
using NetLpr.Core.Values.Preprocessing;

namespace NetLpr.Desktop.Extensions
{
    public static class WritableBitmapExtensions
    {
        public static void WriteImageToUiComponent(this WriteableBitmap temp, IDecodedFrame decodedVideoFrame, NetLpr.Core.Values.Preprocessing.PixelFormat pixelFormat)
        {
            try
            {
                using (var fb = temp.Lock())
                {
                    if (fb.Address == IntPtr.Zero)
                    {
                        System.Diagnostics.Debug.WriteLine($"{nameof(WritableBitmapExtensions)}: FrameBuffer address is null");
                        return;
                    }

                    if (fb.RowBytes <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"{nameof(WritableBitmapExtensions)}: Invalid RowBytes: {fb.RowBytes}");
                        return;
                    }

                    if (fb.Size.Width <= 0 || fb.Size.Height <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"{nameof(WritableBitmapExtensions)}: Invalid frame size: {fb.Size.Width}x{fb.Size.Height}");
                        return;
                    }

                    decodedVideoFrame.TransformTo(fb.Address, fb.RowBytes, pixelFormat);

                }
            }
            catch (Exception)
            {

            }
        }
        public static unsafe WriteableBitmap? GetRoi(this WriteableBitmap original, RectangleF roi)
        {
            int sourceWidth = original.PixelSize.Width;
            int sourceHeight = original.PixelSize.Height;

            int x = Math.Max(0, (int)Math.Round(roi.X));
            int y = Math.Max(0, (int)Math.Round(roi.Y));

            int width = (int)Math.Round(roi.Width);
            if (x + width > sourceWidth) width = sourceWidth - x;

            int height = (int)Math.Round(roi.Height);
            if (y + height > sourceHeight) height = sourceHeight - y;

            if (width <= 0 || height <= 0)
                return null;

            var rect = new PixelRect(x, y, width, height);

            int stride = rect.Width * 4;
            int bufferSize = stride * rect.Height;

            var result = new WriteableBitmap(
                new PixelSize(rect.Width, rect.Height),
                original.Dpi,
                Avalonia.Platform.PixelFormat.Bgra8888,
                AlphaFormat.Premul);

            using var fb = result.Lock();

            original.CopyPixels(
                rect,
                fb.Address,
                bufferSize,
                stride);

            return result;
        }
    }
}
