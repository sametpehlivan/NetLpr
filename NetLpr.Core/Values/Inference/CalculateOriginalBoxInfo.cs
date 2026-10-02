using NetLpr.Core.Values.Preprocessing;

namespace NetLpr.Core.Values.Inference
{
    public class CalculateOriginalBoxInfo
    {
        public System.Drawing.Size OriginalSize { get; private set; }
        public System.Drawing.Size ProcessedSize { get; private set; }
        public double ScaleX { get; private set; }
        public double ScaleY { get; private set; }
        public double ScaledWidth { get; private set; }
        public double ScaledHeight { get; private set; }
        public double PaddingX { get; private set; }
        public double PaddingY { get; private set; }
        public CalculateOriginalBoxInfo(System.Drawing.Size originalSize, System.Drawing.Size processedSize, ScalingPolicy scalingPolicy)
        {
            OriginalSize = originalSize;
            ProcessedSize = processedSize;
            if (scalingPolicy.Equals(ScalingPolicy.RespectAspectRatio))
            {

                double scale = Math.Min((double)processedSize.Width / originalSize.Width,
                            (double)processedSize.Height / originalSize.Height);
                ScaleX = scale;
                ScaleY = scale;
                ScaledWidth = originalSize.Width * ScaleX;
                ScaledHeight = originalSize.Height * ScaleY;
                PaddingX = (processedSize.Width - ScaledWidth) / 2.0;
                PaddingY = (processedSize.Height - ScaledHeight) / 2.0;
            }
            else
            {

                ScaleX = (double)processedSize.Width / originalSize.Width;
                ScaleY = (double)processedSize.Height / originalSize.Height;

                ScaledWidth = originalSize.Width * ScaleX;
                ScaledHeight = originalSize.Height * ScaleY;

                PaddingX = 0;
                PaddingY = 0;
            }
        }
    }
}
