using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Values.Preprocessing;
using OpenCvSharp;

namespace NetLpr.ImageProcessingCv
{
    public static class ConverColorCodeExtensions
    {
        public static ColorConversionCodes? PixelFormatToColorConversions(
            this PixelFormat src,
            PixelFormat dst)
        {
            if (src.Equals(dst))
                return null;
            if (src == PixelFormat.Grayscale)
            {
                if (dst == PixelFormat.Rgb24)
                    return ColorConversionCodes.GRAY2RGB;
                if (dst == PixelFormat.Bgr24)
                    return ColorConversionCodes.GRAY2BGR;
                if (dst == PixelFormat.Rgba32)
                    return ColorConversionCodes.GRAY2RGBA;
                if (dst == PixelFormat.Bgra32)
                    return ColorConversionCodes.GRAY2BGRA;
            }
            if (src == PixelFormat.Rgb24)
            {
                if (dst == PixelFormat.Grayscale)
                    return ColorConversionCodes.RGB2GRAY;
                if (dst == PixelFormat.Bgr24)
                    return ColorConversionCodes.RGB2BGR;
                if (dst == PixelFormat.Rgba32)
                    return ColorConversionCodes.RGB2RGBA;
                if (dst == PixelFormat.Bgra32)
                    return ColorConversionCodes.RGB2BGRA;
            }
            if (src == PixelFormat.Bgr24)
            {
                if (dst == PixelFormat.Grayscale)
                    return ColorConversionCodes.BGR2GRAY;
                if (dst == PixelFormat.Rgb24)
                    return ColorConversionCodes.BGR2RGB;
                if (dst == PixelFormat.Rgba32)
                    return ColorConversionCodes.BGR2RGBA;
                if (dst == PixelFormat.Bgra32)
                    return ColorConversionCodes.BGR2BGRA;
            }
            if (src == PixelFormat.Bgra32)
            {
                if (dst == PixelFormat.Grayscale)
                    return ColorConversionCodes.BGRA2GRAY;
                if (dst == PixelFormat.Rgb24)
                    return ColorConversionCodes.BGRA2RGB;
                if (dst == PixelFormat.Bgr24)
                    return ColorConversionCodes.BGRA2BGR;
                if (dst == PixelFormat.Rgba32)
                    return ColorConversionCodes.BGRA2RGBA;
            }
            if (src == PixelFormat.Rgba32)
            {
                if (dst == PixelFormat.Grayscale)
                    return ColorConversionCodes.RGBA2GRAY;
                if (dst == PixelFormat.Rgb24)
                    return ColorConversionCodes.RGBA2RGB;
                if (dst == PixelFormat.Bgr24)
                    return ColorConversionCodes.RGBA2BGR;
                if (dst == PixelFormat.Bgra32)
                    return ColorConversionCodes.RGBA2BGRA;
            }
            throw new ArgumentOutOfRangeException(
                     nameof(dst),
                     dst,
                     $"Unsupported pixel format conversion: {src} -> {dst}");
        }
    }
}
