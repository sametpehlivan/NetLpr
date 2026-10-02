using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Models;
using NetLpr.Core.Values.Tracking;

namespace NetLpr.Persistence.Sqlite.Models
{
    public class TrackedInfoDbModel
    {
        public string SourceId { get;  set; }
        public string TrackedId { get; set; }
        public DateTime DateTime { get; set; } 
        public string ImagePath { get; set; } 
        public RectangleF PlateRectangle { get; set; }
        public Dictionary<string, List<ScoreAndValue>> TrackingValues { get; set; }
    }
}
