using System.Drawing;
namespace NetLpr.Core.Extensions
{
    public static class RectangleExtensions
    {
        public static RectangleF CreateFromResized(this RectangleF rect, Size originalSize, Size newSize, bool isPadding = false)
        {
            double scaleX, scaleY, offsetX = 0, offsetY = 0;
            if (isPadding)
            {
                double ratioX = (double)newSize.Width / originalSize.Width;
                double ratioY = (double)newSize.Height / originalSize.Height;
                double scale = Math.Min(ratioX, ratioY);

                double scaledWidth = originalSize.Width * scale;
                double scaledHeight = originalSize.Width * scale;


                offsetX = (newSize.Width - scaledWidth) / 2.0;
                offsetY = (newSize.Height - scaledHeight) / 2.0;

                scaleX = scaleY = scale;
            }
            else
            {

                scaleX = (double)newSize.Width / originalSize.Width;
                scaleY = (double)newSize.Height / originalSize.Height;
            }

            return new Rectangle(
                (int)(rect.X * scaleX + offsetX),
                (int)(rect.Y * scaleY + offsetY),
                (int)(rect.Width * scaleX),
                (int)(rect.Height * scaleY)
            );
        }
        public static double CalculatePolygonArea(this RectangleF rect)
        {
            var polygon = new List<(double x, double y)>
            {
                (rect.X, rect.Y),
                (rect.X + rect.Width, rect.Y),
                (rect.X + rect.Width, rect.Y + rect.Height),
                (rect.X, rect.Y + rect.Height)
            };

            if (polygon.Count < 3) return 0;

            double area = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                int j = (i + 1) % polygon.Count;
                area += polygon[i].x * polygon[j].y;
                area -= polygon[j].x * polygon[i].y;
            }

            return Math.Abs(area) / 2.0;
        }
    }
}
