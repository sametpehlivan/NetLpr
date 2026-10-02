using System.Drawing;

namespace NetLpr.Core.Helpers
{
    public static class TrackerUtils
    {

        public static double CalculateIoU(RectangleF bbTest, RectangleF bbGt)
        {
            
            double xx1 = Math.Max(bbTest.X, bbGt.X);
            double yy1 = Math.Max(bbTest.Y, bbGt.Y);
            double xx2 = Math.Min(bbTest.X + bbTest.Width, bbGt.X + bbGt.Width);
            double yy2 = Math.Min(bbTest.Y + bbTest.Height, bbGt.Y + bbGt.Height);

            double w = Math.Max(0, xx2 - xx1);
            double h = Math.Max(0, yy2 - yy1);
            double inter = w * h;
            double union = bbTest.Width * bbTest.Height + bbGt.Width * bbGt.Height - inter;
            return union > 0 ? inter / union : 0;
        }

        public static (int[,], int[], int[]) AssociateDetectionsToTrackers(List<RectangleF> detections, List<RectangleF> trackers, double iouThreshold = 0.3)
        {
            if (trackers.Count == 0)
            {
                return (new int[0, 2], Enumerable.Range(0, detections.Count).ToArray(), new int[0]);
            }

            double[,] iouMatrix = new double[detections.Count, trackers.Count];
            for (int d = 0; d < detections.Count; d++)
                for (int t = 0; t < trackers.Count; t++)
                    iouMatrix[d, t] = CalculateIoU(detections[d], trackers[t]);

            double scale = 1000.0; 
            int[,] costMatrix = new int[iouMatrix.GetLength(0), iouMatrix.GetLength(1)];

            for (int i = 0; i < iouMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < iouMatrix.GetLength(1); j++)
                {
                    costMatrix[i, j] = (int)(-iouMatrix[i, j] * scale);
                }
            }


            var assignment = costMatrix.FindAssignments();

            List<int[]> matchesList = new List<int[]>();
            List<int> unmatchedDetections = new List<int>();
            List<int> unmatchedTrackers = new List<int>();

            for (int d = 0; d < assignment.Length; d++)
            {
                int t = assignment[d];
                if (t == -1 || iouMatrix[d, t] < iouThreshold)
                {
                    unmatchedDetections.Add(d);
                    if (t != -1)
                        unmatchedTrackers.Add(t);
                }
                else
                {
                    matchesList.Add(new int[] { d, t });
                }
            }

            for (int t = 0; t < trackers.Count; t++)
            {
                if (!matchesList.Any(m => m[1] == t) && !unmatchedTrackers.Contains(t))
                    unmatchedTrackers.Add(t);
            }

            int[,] matches = matchesList.Count > 0 ? new int[matchesList.Count, 2] : new int[0, 2];
            for (int i = 0; i < matchesList.Count; i++)
            {
                matches[i, 0] = matchesList[i][0];
                matches[i, 1] = matchesList[i][1];
            }

            return (matches, unmatchedDetections.ToArray(), unmatchedTrackers.ToArray());
        }

     
    }
}
