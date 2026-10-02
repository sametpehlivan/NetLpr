using System;
using NetLpr.Core;
using NetLpr.Core.Localization;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.Sources;
using NetLpr.Core.Services.Syncronizations;
using NetLpr.Core.Values.Preprocessing;
using NetLpr.ImageProcessingCv;
using NetLpr.RealTimeStream.Ffmpeg;
using OpenCvSharp;

namespace NetLpr.Boot.Node
{
    public class SourceFactory
    {
        public static RtspSource CreateSource(ILogger logger, RtspSourceInfo  sourceInfo,ILocalizationService localizationService,RtspSourceContext serviceContext, SynchronizationServiceContext synchronizationServiceContext)
        {
            return new FfmpegRtspStreamReader(
                sourceInfo,
                logger,
                localizationService,
                serviceContext,
                synchronizationServiceContext,
                (frame) =>
                {

                    Mat? mat = null;
                    try
                    {

                        mat = new Mat(new Size(frame.GetSize().Width, frame.GetSize().Height), MatType.CV_8UC3);
                        frame.TransformTo(
                            mat.Data, 
                            (int)mat.Step(),
                            PixelFormat.Rgb24
                        );
                        return new Image(frame.GetFrameCount(),frame.GetSourceId(),mat, PixelFormat.Rgb24, frame.GetStreamAnalysesInfo());
                    }
                    catch
                    {
                        mat?.Dispose();
                        throw;
                    }
                }
            );
        }
    }
}
