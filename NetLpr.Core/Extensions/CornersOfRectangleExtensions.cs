using NetLpr.Core.Values.Tracking;

namespace NetLpr.Core.Extensions
{
    public static class CornersOfRectangleExtensions
    {
        public static List<CornersOfRectangle> GetCorners()
        {
            return
            [
                CornersOfRectangle.LT,
                CornersOfRectangle.RT,
                CornersOfRectangle.LB,
                CornersOfRectangle.RB
            ];
        }
    }
}
