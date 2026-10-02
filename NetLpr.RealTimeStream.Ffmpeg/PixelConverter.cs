using System;
using System.Threading.Tasks;
using NetLpr.Core.Values.Preprocessing;

namespace NetLpr.RealTimeStream.Ffmpeg;

public static class PixelConverter
{
    public static unsafe void Convert(
        PixelFormat srcFormat, byte* src, int srcStride,
        PixelFormat dstFormat, byte* dst, int dstStride,
        int width, int height)
    {
        if (srcFormat == dstFormat && srcStride == dstStride)
        {
            int rowBytes = width * GetBytesPerPixel(srcFormat);
            Parallel.For(0, height, y =>
            {
                byte* s = src + y * (long)srcStride;
                byte* d = dst + y * (long)dstStride;
                Buffer.MemoryCopy(s, d, dstStride, rowBytes);
            });
            return;
        }

        Parallel.For(0, height, y =>
        {
            byte* s = src + y * (long)srcStride;
            byte* d = dst + y * (long)dstStride;

            ConvertRow(srcFormat, s, dstFormat, d, width);
        });
    }

    private static unsafe void ConvertRow(PixelFormat srcFmt, byte* src, PixelFormat dstFmt, byte* dst, int width)
    {
        // BGR24 -> RGB24
        if (srcFmt == PixelFormat.Bgr24 && dstFmt == PixelFormat.Rgb24)
        {
            for (int x = 0; x < width; x++)
            {
                dst[0] = src[2]; // R
                dst[1] = src[1]; // G
                dst[2] = src[0]; // B
                src += 3;
                dst += 3;
            }
            return;
        }

        // BGRA32 -> RGBA32
        if (srcFmt == PixelFormat.Bgra32 && dstFmt == PixelFormat.Rgba32)
        {
            for (int x = 0; x < width; x++)
            {
                dst[0] = src[2]; // R
                dst[1] = src[1]; // G
                dst[2] = src[0]; // B
                dst[3] = src[3]; // A
                src += 4;
                dst += 4;
            }
            return;
        }

        // BGR24 -> RGBA32
        if (srcFmt == PixelFormat.Bgr24 && dstFmt == PixelFormat.Rgba32)
        {
            for (int x = 0; x < width; x++)
            {
                dst[0] = src[2]; // R
                dst[1] = src[1]; // G
                dst[2] = src[0]; // B
                dst[3] = 255;    // A
                src += 3;
                dst += 4;
            }
            return;
        }

        FallbackConvertRow(srcFmt, src, dstFmt, dst, width);
    }

    private static unsafe void FallbackConvertRow(PixelFormat srcFmt, byte* src, PixelFormat dstFmt, byte* dst, int width)
    {
        for (int x = 0; x < width; x++)
        {
            byte r = 0, g = 0, b = 0, a = 255;

            switch (srcFmt)
            {
                case PixelFormat.Grayscale:
                    r = g = b = *src++;
                    break;
                case PixelFormat.Rgb24:
                    r = src[0]; g = src[1]; b = src[2];
                    src += 3;
                    break;
                case PixelFormat.Rgba32:
                    r = src[0]; g = src[1]; b = src[2]; a = src[3];
                    src += 4;
                    break;
                case PixelFormat.Bgr24:
                    b = src[0]; g = src[1]; r = src[2];
                    src += 3;
                    break;
                case PixelFormat.Bgra32:
                    b = src[0]; g = src[1]; r = src[2]; a = src[3];
                    src += 4;
                    break;
            }

            switch (dstFmt)
            {
                case PixelFormat.Grayscale:
                    *dst++ = (byte)((r * 299 + g * 587 + b * 114) / 1000);
                    break;
                case PixelFormat.Rgb24:
                    dst[0] = r; dst[1] = g; dst[2] = b;
                    dst += 3;
                    break;
                case PixelFormat.Rgba32:
                    dst[0] = r; dst[1] = g; dst[2] = b; dst[3] = a;
                    dst += 4;
                    break;
                case PixelFormat.Bgr24:
                    dst[0] = b; dst[1] = g; dst[2] = r;
                    dst += 3;
                    break;
                case PixelFormat.Bgra32:
                    dst[0] = b; dst[1] = g; dst[2] = r; dst[3] = a;
                    dst += 4;
                    break;
            }
        }
    }

    private static int GetBytesPerPixel(PixelFormat fmt) => fmt switch
    {
        PixelFormat.Grayscale => 1,
        PixelFormat.Rgb24 or PixelFormat.Bgr24 => 3,
        PixelFormat.Rgba32 or PixelFormat.Bgra32 => 4,
        _ => 3
    };
}