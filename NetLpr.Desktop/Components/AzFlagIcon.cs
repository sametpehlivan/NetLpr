using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace NetLpr.Desktop.Components
{
    public class AzFlagIcon : UserControl
    {

        public AzFlagIcon()
        {
            Width = 32;
            Height = 24;
            CornerRadius = new CornerRadius(6);
            Content = BuildFlagView();
       
        }

        private Border BuildFlagView()
        {
            var canvas = new Canvas
            {
                Width = 640,
                Height = 480
            };

            canvas.Children.Add(CreatePath(
                "M.1 0h640v480H.1z",
                "#3F9C35"));

            canvas.Children.Add(CreatePath(
                "M.1 0h640v320H.1z",
                "#ED2939"));


            canvas.Children.Add(CreatePath(
                "M.1 0h640v160H.1z",
                "#00B9E4"));

            canvas.Children.Add(CreatePath(
                "M 232 240 a 72 72 0 1 0 144 0 a 72 72 0 1 0 -144 0",
                "#FFFFFF"));
            canvas.Children.Add(CreatePath(
                "M 260 240 a 60 60 0 1 0 120 0 a 60 60 0 1 0 -120 0",
                "#ED2939"));
            canvas.Children.Add(CreatePath(
                "m384 200 7.7 21.5 20.6-9.8-9.8 20.7L424 240l-21.5 7.7 9.8 20.6-20.6-9.8L384 280l-7.7-21.5-20.6 9.8 9.8-20.6L344 240l21.5-7.7-9.8-20.6 20.6 9.8z",
                "#FFFFFF"));

            return new Border
            {
                CornerRadius = CornerRadius,
                ClipToBounds = true,
                Child = new Viewbox
                {
                    Stretch = Stretch.Uniform,
                    Child = canvas
                }
            };
        }

        private static Avalonia.Controls.Shapes.Path CreatePath(string pathData, string hexColor)
        {
            return new Avalonia.Controls.Shapes.Path
            {
                Data = StreamGeometry.Parse(pathData),
                Fill = Brush.Parse(hexColor)
            };
        }
    }
}
