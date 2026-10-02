using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using NetLpr.Desktop.ViewModels;

namespace NetLpr.Desktop.Views
{
    public partial class CameraEditView : UserControl
    {
        private Avalonia.Point _startPoint;
        private bool _isDrawingRoi = false;

        public CameraEditView()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is CameraEditViewModel vm)
            {
                RenderVisuals(vm);
                var target = vm.RtspSourceInfo.StreamAnalysesInfo;
                target.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(target.AnalysesType))
                    {
                        vm.RtspSourceInfo.StreamAnalysesInfo.AnalysesPoints.Clear();
                        vm.IsPointSelectionActive = false;

                        Avalonia.Threading.Dispatcher.UIThread.Post(() => RenderVisuals(vm));
                    }
                };
            }
        }

        private void RoiCanvas_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (DataContext is CameraEditViewModel vm)
            {
                RenderVisuals(vm);
            }
        }


        private Rect GetRenderedImageBounds()
        {
            double canvasWidth = RoiCanvas.Bounds.Width;
            double canvasHeight = RoiCanvas.Bounds.Height;

            if (canvasWidth <= 0 || canvasHeight <= 0)
                return default;
            double imageWidth = TargetImage?.Source?.Size.Width ?? canvasWidth;
            double imageHeight = TargetImage?.Source?.Size.Height ?? canvasHeight;

            if (imageWidth <= 0 || imageHeight <= 0)
                return new Rect(0, 0, canvasWidth, canvasHeight);

            double imageAspect = imageWidth / imageHeight;
            double canvasAspect = canvasWidth / canvasHeight;

            double renderedWidth, renderedHeight;
            double offsetX = 0;
            double offsetY = 0;

            if (canvasAspect > imageAspect)
            {
                renderedHeight = canvasHeight;
                renderedWidth = canvasHeight * imageAspect;
                offsetX = (canvasWidth - renderedWidth) / 2.0;
            }
            else
            {
                renderedWidth = canvasWidth;
                renderedHeight = canvasWidth / imageAspect;
                offsetY = (canvasHeight - renderedHeight) / 2.0;
            }

            return new Rect(offsetX, offsetY, renderedWidth, renderedHeight);
        }

        private void RoiCanvas_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is not CameraEditViewModel vm) return;

            var imgBounds = GetRenderedImageBounds();
            if (imgBounds.Width <= 0 || imgBounds.Height <= 0) return;

            var pointerProps = e.GetCurrentPoint(RoiCanvas).Properties;
            var pos = e.GetPosition(RoiCanvas);

            if (vm.IsRoiSelectionActive && pointerProps.IsLeftButtonPressed)
            {
                _isDrawingRoi = true;
                _startPoint = pos;

                Canvas.SetLeft(RoiRectangle, _startPoint.X);
                Canvas.SetTop(RoiRectangle, _startPoint.Y);
                RoiRectangle.Width = 0;
                RoiRectangle.Height = 0;
                RoiRectangle.IsVisible = true;
                e.Handled = true;
            }
            else if (vm.IsPointSelectionActive)
            {
                if (pointerProps.IsLeftButtonPressed)
                {
                    double scaledX = (pos.X - imgBounds.X) / imgBounds.Width;
                    double scaledY = (pos.Y - imgBounds.Y) / imgBounds.Height;

                    scaledX = Math.Clamp(scaledX, 0.0, 1.0);
                    scaledY = Math.Clamp(scaledY, 0.0, 1.0);

                    vm.TryAddPoint((float)scaledX, (float)scaledY);
                    RenderVisuals(vm);
                    e.Handled = true;
                }
                else if (pointerProps.IsRightButtonPressed)
                {
                    vm.FinishPointSelection();
                    RenderVisuals(vm);
                    e.Handled = true;
                }
            }
        }

        private void RoiCanvas_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDrawingRoi && DataContext is CameraEditViewModel vm && vm.IsRoiSelectionActive)
            {
                var currentPoint = e.GetPosition(RoiCanvas);
                var x = Math.Min(currentPoint.X, _startPoint.X);
                var y = Math.Min(currentPoint.Y, _startPoint.Y);
                var width = Math.Max(currentPoint.X, _startPoint.X) - x;
                var height = Math.Max(currentPoint.Y, _startPoint.Y) - y;

                Canvas.SetLeft(RoiRectangle, x);
                Canvas.SetTop(RoiRectangle, y);
                RoiRectangle.Width = width;
                RoiRectangle.Height = height;
            }
        }

        private void RoiCanvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_isDrawingRoi && DataContext is CameraEditViewModel vm && vm.IsRoiSelectionActive)
            {
                _isDrawingRoi = false;
                var imgBounds = GetRenderedImageBounds();

                if (imgBounds.Width > 0 && imgBounds.Height > 0)
                {
                    double rectLeft = Canvas.GetLeft(RoiRectangle);
                    double rectTop = Canvas.GetTop(RoiRectangle);

                    double scaledX = (rectLeft - imgBounds.X) / imgBounds.Width;
                    double scaledY = (rectTop - imgBounds.Y) / imgBounds.Height;
                    double scaledWidth = RoiRectangle.Width / imgBounds.Width;
                    double scaledHeight = RoiRectangle.Height / imgBounds.Height;

                    scaledX = Math.Clamp(scaledX, 0.0, 1.0);
                    scaledY = Math.Clamp(scaledY, 0.0, 1.0);
                    scaledWidth = Math.Clamp(scaledWidth, 0.0, 1.0 - scaledX);
                    scaledHeight = Math.Clamp(scaledHeight, 0.0, 1.0 - scaledY);

                    vm.UpdateRoi((float)scaledX,(float) scaledY,(float) scaledWidth,(float) scaledHeight);
                    RenderVisuals(vm);
                }
                e.Handled = true;
            }
        }

        private void RenderVisuals(CameraEditViewModel vm)
        {
            var imgBounds = GetRenderedImageBounds();
            if (imgBounds.Width <= 0 || imgBounds.Height <= 0) return;

            double w = imgBounds.Width;
            double h = imgBounds.Height;
            double offsetX = imgBounds.X;
            double offsetY = imgBounds.Y;

            var roi = vm.RtspSourceInfo.StreamAnalysesInfo.AnalysesRoi;
            Canvas.SetLeft(RoiRectangle, offsetX + (roi.X * w));
            Canvas.SetTop(RoiRectangle, offsetY + (roi.Y * h));
            RoiRectangle.Width = roi.Width * w;
            RoiRectangle.Height = roi.Height * h;
            RoiRectangle.IsVisible = true;

            var points = vm.RtspSourceInfo.StreamAnalysesInfo.AnalysesPoints;
            var avaloniaPoints = new Avalonia.Collections.AvaloniaList<Avalonia.Point>();
            var oldDots = RoiCanvas.Children.OfType<Ellipse>().ToList();

            foreach (var dot in oldDots) RoiCanvas.Children.Remove(dot);

            foreach (var p in points)
            {
                double px = offsetX + (p.X * w);
                double py = offsetY + (p.Y * h);
                avaloniaPoints.Add(new Avalonia.Point(px, py));

                var dot = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Yellow };
                Canvas.SetLeft(dot, px - 4);
                Canvas.SetTop(dot, py - 4);
                RoiCanvas.Children.Add(dot);
            }

            var type = vm.RtspSourceInfo.StreamAnalysesInfo.AnalysesType;
            PointsPolyline.IsVisible = false;
            PointsPolygon.IsVisible = false;

            if (points.Count > 0)
            {
                if (vm.IsPointSelectionActive)
                {
                    PointsPolyline.Points = avaloniaPoints;
                    PointsPolyline.IsVisible = true;
                }
                else
                {
                    PointsPolygon.Points = avaloniaPoints;
                    PointsPolygon.IsVisible = true;
                }
            }
        }

        public static Window CreateViewForCreateDialog(string title, CameraEditViewModel dataContext)
        {
            var dialogWindow = new Window
            {
                Title = title,
                Content = new CameraEditView()
                {
                    DataContext = dataContext
                },
                SizeToContent = SizeToContent.WidthAndHeight,
                CanResize = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            return dialogWindow;
        }

        public static Window CreateViewForUpdateDialog(string title, CameraEditViewModel dataContext)
        {
            var view = new CameraEditView()
            {
                DataContext = dataContext
            };
            var dialogWindow = new Window
            {
                Title = title,
                Content = view,
                SizeToContent = SizeToContent.WidthAndHeight,
                CanResize = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            return dialogWindow;
        }


    }
}
