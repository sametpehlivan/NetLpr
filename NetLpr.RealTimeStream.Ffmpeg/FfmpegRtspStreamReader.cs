using System.Drawing;
using NetLpr.Core.Localization;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.Sources;
using NetLpr.Core.Services.Syncronizations;
using NetLpr.Core.Values.Preprocessing;
using FFmpeg.AutoGen;

namespace NetLpr.RealTimeStream.Ffmpeg
{
    public class FfmpegRtspStreamReader : RtspSource
    {
        private CancellationTokenSource? _receiveCts;
        private Task _receiveTask = Task.CompletedTask;
        private DateTime _lastReceiveTime;
     
        private unsafe AVFormatContext* _pFormatContext;
        private unsafe AVCodecContext* _pCodecContext;
        private int _videoStreamIndex;
        private unsafe AVFrame* _rawFrame;
        private unsafe AVPacket* _pPacket;

        private ulong _frameCount = 0;
        private TransformFilter _transformFilter = new TransformFilter();
        private RtspConnectionInfo _lastConnectionInfo = RtspConnectionInfo.Empty;
        private StreamAnalysesInfo _lastStreamAnalysesInfo = new StreamAnalysesInfo();
        private Size _beforeSize = new Size(768,432);



        public FfmpegRtspStreamReader(RtspSourceInfo rtspSourceInfo,ILogger logger, ILocalizationService localizationService,RtspSourceContext sourceServiceContext, SynchronizationServiceContext synchronizationContext, Func<IDecodedFrame, BaseImage> converterFunc) : base(rtspSourceInfo, logger, localizationService,sourceServiceContext, synchronizationContext, converterFunc)
        {

        }

        public override void BeforeOnDispose()
        {
            try
            {
                if (_receiveCts is not null)
                {

                    _receiveCts.CancelAsync().GetAwaiter().GetResult();
                    _receiveTask.GetAwaiter().GetResult();
                    _receiveCts.Dispose();
                    _receiveCts = null;
                    _receiveTask = Task.CompletedTask;
                }

                CleanupFfmpeg();
                _transformFilter.Dispose();
            }
            catch (Exception e)
            {
                MessageReceive(this, new("unexpectedError", $"OnClosing : {e.Message}"));
            }
        }
        private bool IsConnectionInfoChanged() => !_lastConnectionInfo.Equals(RtspSourceInfo.RtspConnectionInfo);


        private void UpdateLastUsedConnectionInfo()
        {
            if (IsConnectionInfoChanged())
            {
                _lastConnectionInfo = RtspSourceInfo.RtspConnectionInfo!;
                MessageReceive(this, new("rtspSource.ffmpegWrapper.connectionInfoChanged", _lastConnectionInfo.IpAddress));

            }
        }



        public override async Task StartAsync()
        {
         
            _receiveCts?.Cancel();
            await _receiveTask;
            _receiveCts = new CancellationTokenSource();
            _receiveTask = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token),_receiveCts.Token);
            
        }
        private unsafe void ConnectFfmpeg()
        {

            AVFormatContext* pFormatContext = null;
            AVDictionary* options = null;
            ffmpeg.av_dict_set(&options, "rtsp_transport", _lastConnectionInfo.TransportType.ToString().ToLowerInvariant(), 0);
            ffmpeg.av_dict_set(&options, "stimeout", "3000000", 0);
            ffmpeg.av_dict_set(&options, "timeout", "3000000", 0);
            ffmpeg.av_dict_set(&options, "rw_timeout", "3000000", 0);

            int openResult = ffmpeg.avformat_open_input(&pFormatContext, _lastConnectionInfo.GetRtspUrl(), null, &options);
            ffmpeg.av_dict_free(&options);

            if (openResult != 0)
            {
                MessageReceive(this, new("rtspSource.error.connectionNotOpened", _lastConnectionInfo.IpAddress, openResult));
                throw new Exception($"Rtsp source could not open! {_lastConnectionInfo.IpAddress}");
            }
             


            _pFormatContext = pFormatContext;

            if (ffmpeg.avformat_find_stream_info(_pFormatContext, null) < 0)
            {
                MessageReceive(this, new("rtspSource.error.notFoundStreamInformation"));
                throw new Exception("Rtsp source not found stream info!");
            }
              

            _videoStreamIndex = -1;
            for (int i = 0; i < _pFormatContext->nb_streams; i++)
            {
                if (_pFormatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    _videoStreamIndex = i;
                    break;
                }
            }

            if (_videoStreamIndex == -1)
            {
                MessageReceive(this, new("rtspSource.error.notFoundStreamInformation"));
                throw new Exception("Rtsp source not found stream info!");
            }
            var pStream = _pFormatContext->streams[_videoStreamIndex];
            var codecId = pStream->codecpar->codec_id;
            var pCodec = ffmpeg.avcodec_find_decoder(codecId);

            if (pCodec == null)
            {
             
                MessageReceive(this, new("rtspSource.error.notFoundSupportedCodec"));
                throw new Exception("Rtsp source not found supported codec!");
            }


            _pCodecContext = ffmpeg.avcodec_alloc_context3(pCodec);
            ffmpeg.avcodec_parameters_to_context(_pCodecContext, pStream->codecpar);

            int codecOpenResult = ffmpeg.avcodec_open2(_pCodecContext, pCodec, null);
            if (codecOpenResult < 0)
            {  
                MessageReceive(this, new("rtspSource.error.codecNotOpened"));
                throw new Exception("Rtsp source codec not opened!");
            }
              
            _rawFrame = ffmpeg.av_frame_alloc();
            _pPacket = ffmpeg.av_packet_alloc();
            MessageReceive(this, new("rtspSource.ffmpegWrapper.connectionSuccess", _lastConnectionInfo.IpAddress));

        }
        private unsafe void ReadFramesLoop(CancellationToken token)
        {
            SetStatus(RtspSourceStatus.STARTING);
            Span<byte> errorBuffer = stackalloc byte[256];
            while (!token.IsCancellationRequested)
            {
                if (IsConnectionInfoChanged())
                    break;
                int readResult = ffmpeg.av_read_frame(_pFormatContext, _pPacket);
                if (readResult < 0)
                {
                    fixed (byte* bufferPtr = errorBuffer)
                    {
                        ffmpeg.av_strerror(readResult, bufferPtr, (ulong)errorBuffer.Length);
                        int nullIdx = errorBuffer.IndexOf((byte)0);
                        int length = nullIdx >= 0 ? nullIdx : errorBuffer.Length;
                        string errorMsg = System.Text.Encoding.UTF8.GetString(errorBuffer.Slice(0, length));
                        _logger.LogError("Connection Error: ({ReadResult}): {ErrorMessage}", readResult, errorMsg);
                    }
                    MessageReceive(this, new("rtspSource.error.readError"));
                    throw new Exception("Rtsp source read error!");
                }

                if (_pPacket->stream_index == _videoStreamIndex)
                {
                    int sendPacketResult = ffmpeg.avcodec_send_packet(_pCodecContext, _pPacket);
                    if (sendPacketResult == 0)
                    {
                        while (ffmpeg.avcodec_receive_frame(_pCodecContext, _rawFrame) == 0)
                        {
                            if (_rawFrame != null)
                            {
                                ProcessFrame(_rawFrame);
                            }
                                
                        }
                    }
                }

                ffmpeg.av_packet_unref(_pPacket);
            }
        }
        private unsafe void CleanupFfmpeg()
        {
            if (_rawFrame != null)
            {
                var pFrame = _rawFrame;
                ffmpeg.av_frame_free(&pFrame);
                _rawFrame = null;
            }

            if (_pPacket != null)
            {
                var pPacket = _pPacket;
                ffmpeg.av_packet_free(&pPacket);
                _pPacket = null;
            }

            if (_pCodecContext != null)
            {
                var pCodecContext = _pCodecContext;
                ffmpeg.avcodec_free_context(&pCodecContext);
                _pCodecContext = null;
            }

            if (_pFormatContext != null)
            {
                var pFormatContext = _pFormatContext;
                ffmpeg.avformat_close_input(&pFormatContext);
                _pFormatContext = null;
            }
        }
        private async Task ReceiveLoopAsync(CancellationToken token, Func<Task>? afterConnected = null)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        UpdateLastUsedConnectionInfo();
                        if (token.IsCancellationRequested) break;
                        SetStatus(RtspSourceStatus.CONNECTING);
                        ConnectFfmpeg();
                        if (token.IsCancellationRequested) break;
                        SetStatus(RtspSourceStatus.CONNECTED);
                        if (afterConnected != null)
                            await afterConnected.Invoke();
                        ReadFramesLoop(token);
                    }
                    catch (Exception e)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                        SetStatus(RtspSourceStatus.DISCONNECTED,e.Message);

                        CleanupFfmpeg();
                        try
                        {
                            await Task.Delay(1000, token);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch(Exception e)
            {
                if (!token.IsCancellationRequested)
                {
                    SetStatus(RtspSourceStatus.DISCONNECTED,e.Message);
                }
            }
            finally
            {
                CleanupFfmpeg();
            }
        }
        private unsafe void ProcessFrame(AVFrame* avFrame)
        {
            if (DateTime.UtcNow - _lastReceiveTime > TimeSpan.FromSeconds(5))
                _lastReceiveTime = DateTime.UtcNow;

            var _scaledFrame = new RawFrame(
                  _transformFilter.Transform(_rawFrame, _beforeSize, ScalingPolicy.RespectAspectRatio, PixelFormat.Rgb24),
                  _frameCount++,
                  RtspSourceInfo.Id,
                  PixelFormat.Rgb24,
                  RtspSourceInfo.StreamAnalysesInfo
              );
    
            _decodedFrameHandler?.Invoke(this, _scaledFrame);
        }

        public override void UpdateRtspSourceInfo(RtspSourceInfo sourceInfo)
        {
            sourceInfo.Validate();
            RtspSourceInfo.RtspConnectionInfo = sourceInfo.RtspConnectionInfo;
            RtspSourceInfo.StreamAnalysesInfo = sourceInfo.StreamAnalysesInfo;

        }
    }
}
