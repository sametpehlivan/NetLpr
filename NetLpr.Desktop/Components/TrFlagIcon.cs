using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace NetLpr.Desktop.Components
{
    public class TrFlagIcon : UserControl
    {
        public TrFlagIcon()
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
                "M0 0h640v480H0z",
                "#E30A17"));

            canvas.Children.Add(CreatePath(
                "M407 247.5c0 66.2-54.6 119.9-122 119.9s-122-53.7-122-120 54.6-119.8 122-119.8 122 53.7 122 119.9",
                "#FFFFFF"));

            canvas.Children.Add(CreatePath(
                "M413 247.5c0 53-43.6 95.9-97.5 95.9s-97.6-43-97.6-96 43.7-95.8 97.6-95.8 97.6 42.9 97.6 95.9z",
                "#E30A17"));


            canvas.Children.Add(CreatePath(
                "m430.7 191.5-1 44.3-41.3 11.2 40.8 14.5-1 40.7 26.5-31.8 40.2 14-23.2-34.1 28.3-33.9-43.5 12-25.8-37z",
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