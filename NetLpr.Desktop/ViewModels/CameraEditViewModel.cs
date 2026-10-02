using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using NetLpr.Core.Exceptions;
using NetLpr.Core.Extensions;
using NetLpr.Core.Models;
using NetLpr.Desktop.Extensions;
using NetLpr.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace NetLpr.Desktop.ViewModels
{
    public partial class CameraEditViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _message = string.Empty;
        [ObservableProperty]
        private bool _isEditMode;
        [ObservableProperty]
        private bool _isRoiSelectionActive;
        [ObservableProperty]
        private bool _isPointSelectionActive;
        [ObservableProperty]
        private WriteableBitmap? _currentFrame;
        [ObservableProperty]
        private RtspSourceInfoModel _rtspSourceInfo;
        private readonly Func<RtspSourceInfoModel, Task> _action;
        public CameraEditViewModel(
           Func<RtspSourceInfoModel, Task>? action = null,
           RtspSourceInfoModel? rtspSourceInfo = null)
        {
            _action = action ?? (_ => Task.CompletedTask);
            _isEditMode = rtspSourceInfo != null;
            _rtspSourceInfo = rtspSourceInfo ?? new RtspSourceInfoModel();

        }
        [RelayCommand]
        public async Task Save()
        {
            Message = string.Empty;
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {

                try
                {
                    await _action.Invoke(RtspSourceInfo);
                }
                catch (LocalizationException e)
                {
                    Message = string.Join("\n", e.MessageKeys
                        .Select(x => LocalizationService?.GetLocalizeMessage(x.Message, x.Args) ?? string.Empty));
                }
                catch (Exception ex)
                {
                    Message = LocalizationService?.GetLocalizeMessageWithDefaultMessage("unexpectedError", ex.Message, ex.Message) ?? string.Empty;
                }

            });
        }

        public void UpdateRoi(float x, float y, float width, float height)
        {
            IsRoiSelectionActive = false;

            RtspSourceInfo.StreamAnalysesInfo.AnalysesRoi = new RectangleModel { X = x, Y = y, Width = width, Height = height };

            RtspSourceInfo.StreamAnalysesInfo.AnalysesPoints.Clear();
            IsPointSelectionActive = false;

            Message = LocalizationService?.GetLocalizeMessage("desktop.editCamera.analyseInfo.roiSelected", $"{x:f2}", $"{y:f2}", $"{width:f2}", $"{height:f2}") ?? string.Empty;
        }

        [RelayCommand]
        public void EnableRoiSelection()
        {
            IsPointSelectionActive = false;
            IsRoiSelectionActive = true;
            Message = LocalizationService?.GetLocalizeMessage("desktop.editCamera.analyseInfo.selectRoi") ?? string.Empty;
        }
        [RelayCommand]
        public void EnablePointSelection()
        {
            IsRoiSelectionActive = false;
            IsPointSelectionActive = true;
            RtspSourceInfo.StreamAnalysesInfo.AnalysesPoints.Clear();
            Message = LocalizationService?.GetLocalizeMessage("desktop.editCamera.analyseInfo.roiBoundaryPointInstruction") ?? string.Empty;
        }

        public void TryAddPoint(float x, float y)
        {
            var roi = RtspSourceInfo.StreamAnalysesInfo.AnalysesRoi;

            if (x < roi.X || x > roi.X + roi.Width || y < roi.Y || y > roi.Y + roi.Height)
            {
                Message = LocalizationService?.GetLocalizeMessage("desktop.editCamera.analyseInfo.pointOutsideRoiError") ?? string.Empty;
                return;
            }

            RtspSourceInfo.StreamAnalysesInfo.AnalysesPoints.Add(new PointModel { X = x, Y = y });
            var type = RtspSourceInfo.StreamAnalysesInfo.AnalysesType;
            int count = RtspSourceInfo.StreamAnalysesInfo.AnalysesPoints.Count;
            if (RtspSourceInfo.StreamAnalysesInfo.AnalysesType == StreamAnalysesType.TrackingVehicle)
            {
                if (count <= 2)
                    Message = LocalizationService?.GetLocalizeMessage("desktop.editCamera.analyseInfo.areaSelectPoints") ?? string.Empty;

                else
                    Message = LocalizationService?.GetLocalizeMessage("desktop.editCamera.analyseInfo.areaCompletionInstruction") ?? string.Empty;
            }
            
        }

        public void FinishPointSelection()
        {
            if (!IsPointSelectionActive) return;

            var type = RtspSourceInfo.StreamAnalysesInfo.AnalysesType;
            if (RtspSourceInfo.StreamAnalysesInfo.AnalysesPoints.Count < 3)
            {
                Message = LocalizationService?.GetLocalizeMessage("desktop.editCamera.analyseInfo.areaCompletionInstruction") ?? string.Empty;
                return;
            }

            IsPointSelectionActive = false;
            Message = LocalizationService?.GetLocalizeMessage("desktop.editCamera.analyseInfo.areaCreated") ?? string.Empty;

        }

        public void UpdateFrame(object? sender, WriteableBitmap image)
        {
            CurrentFrame = image;
            RtspSourceInfo.FrameTransformSize = new SizeModel() {
                Width = (int)image.Size.Width,
                Height = (int)image.Size.Height
            };
        }
    }
}
