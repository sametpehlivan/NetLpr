
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
namespace NetLpr.Desktop.Components
{
    public class UsFlagIcon : UserControl
    {


        public UsFlagIcon()
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
                fillHex: "#BD3D44"));

            canvas.Children.Add(CreatePath(
                "M0 55.3h640M0 129h640M0 203h640M0 277h640M0 351h640M0 425h640",
                fillHex: null,
                strokeHex: "#FFFFFF",
                strokeThickness: 37));

            canvas.Children.Add(CreatePath(
                "M0 0h364.8v258.5H0z",
                fillHex: "#192F5D"));

            canvas.Children.Add(CreatePath(
                Generate50StarsPathData(),
                fillHex: "#FFFFFF"));

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

        private static string Generate50StarsPathData()
        {
            var sb = new StringBuilder();

            int[][] starGrid = new int[][]
            {
                new int[] { 16, 77, 138, 199, 260, 320 }, 
                new int[] { 47, 108, 169, 229, 290 },     
                new int[] { 16, 77, 138, 199, 260, 320 }, 
                new int[] { 47, 108, 169, 229, 290 },    
                new int[] { 16, 77, 138, 199, 260, 320 },
                new int[] { 47, 108, 169, 229, 290 },     
                new int[] { 16, 77, 138, 199, 260, 320 }, 
                new int[] { 47, 108, 169, 229, 290 },    
                new int[] { 16, 77, 138, 199, 260, 320 } 
            };

            int[] yCoords = { 11, 37, 63, 89, 115, 141, 166, 192, 218 };

            for (int row = 0; row < starGrid.Length; row++)
            {
                int y = yCoords[row];
                foreach (int x in starGrid[row])
                {
                    sb.Append($"M{x + 14} {y}l9 27L{x} {y + 10}h28L{x + 5} {y + 27}z ");
                }
            }

            return sb.ToString();
        }

        private static Avalonia.Controls.Shapes.Path CreatePath(
            string pathData,
            string? fillHex = null,
            string? strokeHex = null,
            double strokeThickness = 0)
        {
            var path = new Avalonia.Controls.Shapes.Path
            {
                Data = StreamGeometry.Parse(pathData)
            };

            if (!string.IsNullOrEmpty(fillHex))
            {
                path.Fill = Brush.Parse(fillHex);
            }

            if (!string.IsNullOrEmpty(strokeHex) && strokeThickness > 0)
            {
                path.Stroke = Brush.Parse(strokeHex);
                path.StrokeThickness = strokeThickness;
            }

            return path;
        }
    }
}