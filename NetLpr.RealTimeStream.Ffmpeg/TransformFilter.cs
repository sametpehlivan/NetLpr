namespace NetLpr.RealTimeStream.Ffmpeg;
using System;
using System.Drawing;
using NetLpr.Core.Values.Preprocessing;
using FFmpeg.AutoGen;

public class TransformFilter : IDisposable
{
    private unsafe SwsContext* _swsContext = null;
    private unsafe AVFrame* _dstFrame = null;

    private int _cachedDstW = 0;
    private int _cachedDstH = 0;
    private AVPixelFormat _cachedTargetFormat = AVPixelFormat.AV_PIX_FMT_NONE;

    public unsafe AVFrame* Transform(
        AVFrame* srcFrame,
        Size size,
        ScalingPolicy scalingPolicy,
        PixelFormat pixelFormat
        )
    {
        if (srcFrame == null)
            throw new ArgumentNullException(nameof(srcFrame));

        RectangleF normalizedRoi = new RectangleF(0, 0, 1, 1);

        (int srcX, int srcY, int srcW, int srcH) = GetAbsoluteRoi(srcFrame, normalizedRoi);
        (int dstW, int dstH) = GetScaledSize(scalingPolicy, srcW, srcH, size.Width,size.Height);

        AVPixelFormat targetPixelFormat = MapPixelFormat(pixelFormat);
        int swsFlags = MapScalingQuality(ScalingQuality.Fastbilinear);
        _swsContext = ffmpeg.sws_getCachedContext(
            _swsContext,
            srcW, srcH, (AVPixelFormat)srcFrame->format,
            dstW, dstH, targetPixelFormat,
            swsFlags, null, null, null
        );

        if (_swsContext == null)
            throw new InvalidOperationException("SwsContext oluşturulamadı veya güncellenemedi.");

        EnsureDestinationFrame(dstW, dstH, targetPixelFormat);

        byte_ptrArray4 srcData = new byte_ptrArray4();
        int_array4 srcLinesize = new int_array4();
        CalculateRoiPointers(srcFrame, srcX, srcY, ref srcData, ref srcLinesize);

        byte_ptrArray4 dstData = new byte_ptrArray4();
        int_array4 dstLinesize = new int_array4();
        for (uint i = 0; i < 4; i++)
        {
            dstData[i] = _dstFrame->data[i];
            dstLinesize[i] = _dstFrame->linesize[i];
        }

        ffmpeg.sws_scale(
            _swsContext,
            srcData, srcLinesize,
            0, srcH,
            dstData, dstLinesize
        );

        return _dstFrame;
    }

    private unsafe void EnsureDestinationFrame(int dstW, int dstH, AVPixelFormat targetPixelFormat)
    {
        if (_dstFrame == null || _cachedDstW != dstW || _cachedDstH != dstH || _cachedTargetFormat != targetPixelFormat)
        {
            if (_dstFrame != null)
            {
                fixed (AVFrame** pDstFrame = &_dstFrame)
                {
                    ffmpeg.av_frame_free(pDstFrame);
                }
            }

            _dstFrame = ffmpeg.av_frame_alloc();
            if (_dstFrame == null)
                throw new InvalidOperationException("AVFrame nesnesi oluşturulamadı.");

            _dstFrame->width = dstW;
            _dstFrame->height = dstH;
            _dstFrame->format = (int)targetPixelFormat;

            int ret = ffmpeg.av_frame_get_buffer(_dstFrame, 32);
            if (ret < 0)
            {
                fixed (AVFrame** pDstFrame = &_dstFrame)
                {
                    ffmpeg.av_frame_free(pDstFrame);
                }
                throw new InvalidOperationException($"Frame buffer tahsis edilemedi. Hata Kodu: {ret}");
            }

            _cachedDstW = dstW;
            _cachedDstH = dstH;
            _cachedTargetFormat = targetPixelFormat;
        }
    }

    private (int dstW, int dstH) GetScaledSize(ScalingPolicy scalingPolicy, int srcW, int srcH, double targetWidth, double targetHeight)
    {
        int targetW = (int)targetWidth;
        int targetH = (int)targetHeight;

        if (targetW <= 0 || targetH <= 0)
            return (Math.Max(2, srcW & ~1), Math.Max(2, srcH & ~1));

        int dstW;
        int dstH;

        switch (scalingPolicy)
        {
            case ScalingPolicy.Stretch:
                dstW = targetW;
                dstH = targetH;
                break;

            case ScalingPolicy.RespectAspectRatio:
                double srcRatio = (double)srcW / srcH;
                double targetRatio = (double)targetW / targetH;

                if (srcRatio > targetRatio)
                {
                    dstW = targetW;
                    dstH = (int)Math.Round(targetW / srcRatio);
                }
                else
                {
                    dstH = targetH;
                    dstW = (int)Math.Round(targetH * srcRatio);
                }
                break;

            default:
                dstW = targetW;
                dstH = targetH;
                break;
        }

        dstW = Math.Max(2, dstW & ~1);
        dstH = Math.Max(2, dstH & ~1);

        return (dstW, dstH);
    }

    private unsafe (int srcX, int srcY, int srcW, int srcH) GetAbsoluteRoi(AVFrame* frame, RectangleF roi)
    {
        int srcWidth = frame->width;
        int srcHeight = frame->height;

        int srcX = (int)(roi.X * srcWidth);
        int srcY = (int)(roi.Y * srcHeight);
        int srcW = (int)(roi.Width * srcWidth);
        int srcH = (int)(roi.Height * srcHeight);

        srcX = Math.Clamp(srcX, 0, srcWidth - 1);
        srcY = Math.Clamp(srcY, 0, srcHeight - 1);
        srcW = Math.Clamp(srcW, 1, srcWidth - srcX);
        srcH = Math.Clamp(srcH, 1, srcHeight - srcY);

        srcX &= ~1;
        srcY &= ~1;
        srcW &= ~1;
        srcH &= ~1;

        if (srcW == 0) srcW = 2;
        if (srcH == 0) srcH = 2;

        return (srcX, srcY, srcW, srcH);
    }

    private unsafe void CalculateRoiPointers(AVFrame* srcFrame, int srcX, int srcY, ref byte_ptrArray4 srcData, ref int_array4 srcLinesize)
    {
        AVPixelFormat fmt = (AVPixelFormat)srcFrame->format;

        for (uint i = 0; i < 4; i++)
        {
            srcData[i] = srcFrame->data[i];
            srcLinesize[i] = srcFrame->linesize[i];
        }

        if (srcX == 0 && srcY == 0) return;

        switch (fmt)
        {
            case AVPixelFormat.AV_PIX_FMT_RGB24:
            case AVPixelFormat.AV_PIX_FMT_BGR24:
                srcData[0] += (srcY * srcFrame->linesize[0]) + (srcX * 3);
                break;
            case AVPixelFormat.AV_PIX_FMT_BGRA:
            case AVPixelFormat.AV_PIX_FMT_RGBA:
            case AVPixelFormat.AV_PIX_FMT_ARGB:
                srcData[0] += (srcY * srcFrame->linesize[0]) + (srcX * 4);
                break;
            case AVPixelFormat.AV_PIX_FMT_GRAY8:
                srcData[0] += (srcY * srcFrame->linesize[0]) + srcX;
                break;
            case AVPixelFormat.AV_PIX_FMT_YUV420P:
            case AVPixelFormat.AV_PIX_FMT_YUVJ420P:
                srcData[0] += (srcY * srcFrame->linesize[0]) + srcX;
                srcData[1] += ((srcY / 2) * srcFrame->linesize[1]) + (srcX / 2);
                srcData[2] += ((srcY / 2) * srcFrame->linesize[2]) + (srcX / 2);
                break;
        }
    }

    private AVPixelFormat MapPixelFormat(PixelFormat format) => format switch
    {
        PixelFormat.Grayscale => AVPixelFormat.AV_PIX_FMT_GRAY8,
        PixelFormat.Rgb24 => AVPixelFormat.AV_PIX_FMT_BGR24,
        PixelFormat.Rgba32 => AVPixelFormat.AV_PIX_FMT_RGBA,
        PixelFormat.Bgr24 => AVPixelFormat.AV_PIX_FMT_RGB24,
        PixelFormat.Bgra32 => AVPixelFormat.AV_PIX_FMT_BGRA,
        _ => AVPixelFormat.AV_PIX_FMT_BGR24
    };

    private int MapScalingQuality(ScalingQuality quality) => quality switch
    {
        ScalingQuality.Nearest => ffmpeg.SWS_POINT,
        ScalingQuality.Linear => ffmpeg.SWS_BILINEAR,
        ScalingQuality.Bilinear => ffmpeg.SWS_BILINEAR,
        ScalingQuality.Bicubic => ffmpeg.SWS_BICUBIC,
        ScalingQuality.Fastbilinear => ffmpeg.SWS_FAST_BILINEAR,
        _ => ffmpeg.SWS_FAST_BILINEAR
    };

    private unsafe void DisposeUnsafe()
    {
        if (_swsContext != null)
        {
            ffmpeg.sws_freeContext(_swsContext);
            _swsContext = null;
        }

        if (_dstFrame != null)
        {
            fixed (AVFrame** pDstFrame = &_dstFrame)
            {
                ffmpeg.av_frame_free(pDstFrame);
            }
        }
    }

    public void Dispose()
    {
        DisposeUnsafe();
        GC.SuppressFinalize(this);
    }

    ~TransformFilter()
    {
        DisposeUnsafe();
    }
}