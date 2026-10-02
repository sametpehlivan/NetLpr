using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLpr.Core.Values.Inference
{
    public class PlateDetectionResponseConfigs
    {
        public float Threshold { get; set; }
        public int DetectMaxObjectPerFrame { get; set; }
        public int ColumnCount { get; }
        public string GenericClassName { get; }
        public Dictionary<int, string> ValidChildClasses { get; }

        public PlateDetectionResponseConfigs(Dictionary<int, string> validChildClasses, string genericClassName = "plate", int columnCount = 7, float threshold = 0.5f, int detectMaxObjectPerFrame = 10)
        {
            ColumnCount = columnCount;
            GenericClassName = genericClassName;
            ValidChildClasses = validChildClasses;
            Threshold = threshold;
            DetectMaxObjectPerFrame = detectMaxObjectPerFrame;
        }
    }
}
