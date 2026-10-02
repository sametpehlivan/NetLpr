using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;


namespace NetLpr.Desktop.Models
{
    public partial class RectangleModel : ObservableObject
    {
        [ObservableProperty]
        private float _width = 0;
        [ObservableProperty]
        private float _height = 0;
        [ObservableProperty]
        private float _x = 0;
        [ObservableProperty]
        private float _y = 0;

     
        public static RectangleModel FromModel(RectangleF rectangle)
        {
            return new RectangleModel { X = rectangle.X, Y = rectangle.Y, Width = rectangle.Width, Height = rectangle.Height };
        }
        public RectangleF ToModel()
        {
            return new RectangleF(X, Y, Width, Height);
        }
    }
}
