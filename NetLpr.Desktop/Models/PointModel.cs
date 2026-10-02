using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;


namespace NetLpr.Desktop.Models
{
    public partial class PointModel : ObservableObject
    {
        [ObservableProperty]
        private float _x = 0;
        [ObservableProperty]
        private float _y = 0;


        public PointF ToModel()
        {
            return new PointF(X, Y);
        }
        public static PointModel FromModel(PointF point) { 
            return new PointModel() { X = point.X, Y = point.Y }; 
        }
    }
}
