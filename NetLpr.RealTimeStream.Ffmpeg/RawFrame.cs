using System;
using System.Drawing;
using System.Security.Cryptography;
using NetLpr.Core.Models;
using NetLpr.Core.Values.Preprocessing;
using FFmpeg.AutoGen;
namespace NetLpr.RealTimeStream.Ffmpeg
{
    public class RawFrame : IDecodedFrame
    {
        private readonly object _lockObject = new object();
        private unsafe AVFrame* _pFrame;
        private Size _size = Size.Empty;
        private int _lineSize;
        private string _sourceId;
        private ulong _frameCount;
        private PixelFormat _pixelFormat;
        private StreamAnalysesInfo _streamAnalysesInfo;
        public unsafe RawFrame(AVFrame* pFrame, ulong frameCount, string sourceId,PixelFormat pixelFormat,StreamAnalysesInfo streamAnalysesInfo)
        {
            if (pFrame == null)
                throw new Exception(nameof(pFrame));
            _pFrame = pFrame;
            _size = new Size(_pFrame->width, _pFrame->height);
            _lineSize = _pFrame->linesize[0];
            _sourceId = sourceId;
            _frameCount = frameCount;
            _pixelFormat = pixelFormat;
            _streamAnalysesInfo = streamAnalysesInfo;
        }
        public  Size GetSize()
        {
            return _size;
        }
        public  PixelFormat GetPixelFormat()
        {
            return _pixelFormat;
        }

        public  unsafe byte* GetPointer()
        {
            return _pFrame->data[0];
        }

        public  int GetStride()
        {

            return _lineSize;
        }


        public void TransformTo(nint dst, int bufferStride, PixelFormat pixelFormat)
        {
            TransformUnsafe(dst, bufferStride, pixelFormat);
        }
        public unsafe void TransformUnsafe(nint dst, int bufferStride, PixelFormat pixelFormat)
        {
            PixelConverter.Convert(GetPixelFormat(), GetPointer(), GetStride(), pixelFormat, (byte*)dst, bufferStride, GetSize().Width, GetSize().Height);

        }

        public StreamAnalysesInfo GetStreamAnalysesInfo()
        {
            return _streamAnalysesInfo;
        }

        public ulong GetFrameCount()
        {
            return _frameCount;
        }
        public string GetSourceId()
        {
            return _sourceId;
        }
    }
}
