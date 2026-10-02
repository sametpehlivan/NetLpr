using System.Drawing;
namespace NetLpr.Core.Models
{
    public enum StreamAnalysesType
    {
        TrackingPlate,
        TrackingVehicle
    }
    public class StreamAnalysesInfo
    {
        public bool IsAnalysesOpen { get; set; } = false;
        public RectangleF AnalysesRoi { get; set; } = new RectangleF(0,0,1,1);
        public StreamAnalysesType AnalysesType { get; set; } = StreamAnalysesType.TrackingPlate;
        public List<PointF> AnalysesPoints { get; set; } = new List<PointF>();
        public bool RoiIsFullOrEmpty()
        {
            return RectangleF.Empty.Equals(AnalysesRoi) || AnalysesRoi.X == 0 && AnalysesRoi.Y == 0 && AnalysesRoi.Width == 1 && AnalysesRoi.Height == 1;
        }
        public bool Equals(StreamAnalysesInfo? other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other is null)
                return false;

            return AnalysesRoi == other.AnalysesRoi
                && AnalysesType == other.AnalysesType
                && AnalysesPoints.SequenceEqual(other.AnalysesPoints);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as StreamAnalysesInfo);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                AnalysesRoi,
                AnalysesType,
                AnalysesPoints
            );
        }
        public List<string> Validate()
        {
            List<string> errors = new();
            if (StreamAnalysesType.TrackingVehicle == AnalysesType && AnalysesPoints.Count() <= 2)
                errors.Add("streamAnalysesInfo.error.trackingVehicleBased.pointsCountShouldBeGraterThanTwo");
            if(StreamAnalysesType.TrackingPlate == AnalysesType && AnalysesPoints.Count() != 0 && AnalysesPoints.Count() <= 2)
                errors.Add("streamAnalysesInfo.error.trackingPlateBased.pointsCountShouldBeGraterThanTwo");
            return errors;
        }
    

    }
}
