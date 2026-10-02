using NetLpr.Core.Values.Preprocessing;
using OpenCvSharp;

namespace NetLpr.ImageProcessingCv
{
    public static class Extensions
    {
       
        public static Scalar ToScalar(this PaddingColor color)
        {
            return new Scalar(color.B, color.G, color.R);
        }

    }
}
