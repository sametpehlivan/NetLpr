namespace NetLpr.Core.Extensions
{
    public static class PointExtensions
    {
        public static bool AreaIsValid(this List<(int x, int y)> area)
        {
            if (area == null)
            {
                return false;
            }
            return area.Count() > 3;
        }
        public static bool IsPointInPolygonD(this (double x,double y) point,List<(int x,int y)> polygon) {
        
              return ((int)point.x,(int)point.y).IsPointInPolygon(polygon);
        }
        public static bool IsPointInPolygon(this (int x, int y) point, List<(int x, int y)> polygon)
        {
            bool inside = false;
            int j = polygon.Count - 1; 

            for (int i = 0; i < polygon.Count; i++)
            {
                var pi = polygon[i];
                var pj = polygon[j];

                bool intersect = pi.y > point.y != pj.y > point.y &&
                                 point.x < (pj.x - pi.x) * (point.y - pi.y) / (double)(pj.y - pi.y) + pi.x;
                if (intersect)
                    inside = !inside;
                j = i;
            }
            return inside;
        }
    }
}
