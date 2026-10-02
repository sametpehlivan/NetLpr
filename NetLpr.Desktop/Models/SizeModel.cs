using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;


namespace NetLpr.Desktop.Models
{
    public partial class SizeModel : ObservableObject
    {
        [ObservableProperty]
        private int _width = 0;
        [ObservableProperty]
        private int _height = 0;
        public static SizeModel FromModel(Size model)
        {
            return new SizeModel()
            {
                Width = model.Width,
                Height = model.Height,
            };
        }
        public Size ToModel()
        {
            return new Size(Width, Height);
        }
    }
}
