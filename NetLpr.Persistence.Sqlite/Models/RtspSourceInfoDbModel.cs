using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace NetLpr.Persistence.Sqlite.Models
{
    public class StreamAnalysesAreaPointDbModel
    {
        public int Id { get; set; } 
        public string SourceId { get; set; } = string.Empty;
        public float X { get; set; }
        public float Y { get; set; }
    }
    public class RtspSourceInfoDbModel
    {
        public string Id { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PathAndQuery { get; set; } = string.Empty;
        public string TransportType { get; set; } = RtspTransportType.TCP.ToString().ToLowerInvariant();
        public float RoiX { get; set; }
        public float RoiY { get; set; }
        public float RoiHeight { get; set; }
        public float RoiWidth { get; set; }
        public bool IsAnalysesOpen { get; set; }
        public string AnalysesType { get; set; } = StreamAnalysesType.TrackingPlate.ToString().ToLowerInvariant();
        public virtual Collection<StreamAnalysesAreaPointDbModel> AnalysesArea { get; set; } = new();

        public RtspSourceInfo ToModel()
        {
            return new RtspSourceInfo
            {
                Id = Id,
                RtspConnectionInfo = new RtspConnectionInfo
                {
                    IpAddress = IpAddress,
                    Port = Port,
                    Username = Username,
                    Password = Password,
                    PathAndQuery = PathAndQuery,
                    TransportType = TransportType.ToLowerInvariant().Equals(RtspTransportType.TCP.ToString().ToLowerInvariant())
                    ? RtspTransportType.TCP
                    : RtspTransportType.UDP,
                },
                StreamAnalysesInfo = new StreamAnalysesInfo
                {
                    IsAnalysesOpen = IsAnalysesOpen,
                    AnalysesType = AnalysesType.ToLowerInvariant().Equals(StreamAnalysesType.TrackingPlate.ToString().ToLowerInvariant())
                    ? StreamAnalysesType.TrackingPlate
                    : StreamAnalysesType.TrackingVehicle,
                    AnalysesRoi = new System.Drawing.RectangleF(RoiX, RoiY, RoiWidth, RoiHeight),
                    AnalysesPoints = AnalysesArea.Select(x => new System.Drawing.PointF(x.X, x.Y)).ToList(),
                }
            };
        }
    }
}



