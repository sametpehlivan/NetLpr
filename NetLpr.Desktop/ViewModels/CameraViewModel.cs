using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using NetLpr.Core.Models;
using NetLpr.Core.Values.Request;
using NetLpr.Core.Values.Response;
using NetLpr.Desktop.Extensions;
using NetLpr.Desktop.Models;
using NetLpr.Desktop.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace NetLpr.Desktop.ViewModels
{
    public partial class CameraViewModel : ViewModelBase
    {
        public ObservableCollection<TrackedInfoResultViewModel> Results { get; set; } = new();
        public string RtspSourceId { get; }
        private string _id = string.Empty;
        private EventHandler<WriteableBitmap> _videoFrameEvent = (s,d) => { };

        [ObservableProperty]
        private bool _videoIsShowing = true;

        [ObservableProperty]
        private string _rtspSourceMessage = string.Empty;

        [ObservableProperty]
        private WriteableBitmap _resultImage;
        private WriteableBitmap _resultImageTemp1;
        private WriteableBitmap _resultImageTemp2;
        private bool _resultImageSelector = true;
        [ObservableProperty]
        private TrackedInfoResultViewModel _trackedInfoResult = new();


        [ObservableProperty]
        private WriteableBitmap _currentFrame;
        private WriteableBitmap _currentFrameTemp1;
        private WriteableBitmap _currentFrameTemp2;
        private bool _currentFrameSelecting = true;

        private NetLpr.Core.Values.Preprocessing.PixelFormat _uiPixelFormat = NetLpr.Core.Values.Preprocessing.PixelFormat.Rgba32;
        private object _frameLock = new object();
        private object _trackingLock = new object();
        private CameraEditViewModel? _currentEditViewModel;
        private Window? _dialog;
        public CameraViewModel(string rtspSourceId)
        {
            RtspSourceId = rtspSourceId;
            _id = $"CameraViewModel-{rtspSourceId}";
            _currentFrameTemp1 = CreateBitmap(640, 400);
            _currentFrameTemp2 = CreateBitmap(640, 400);
            _currentFrame = _currentFrameTemp1;
            _resultImageTemp1 = CreateBitmap(640, 400);
            _resultImageTemp2 = CreateBitmap(640, 400);
            _resultImage = _resultImageTemp1;
            Subscribes();
        }
        ~CameraViewModel()
        {
            DesktopApplicationContext?
                .GetMessageBus()
                .Unsubscribe<UpdateNodeResponse>(_id);
        }
        private void Subscribes()
        {

            if (DesktopApplicationContext != null)
            {
                var node = DesktopApplicationContext.GetNodeManagerService().GetRtspNode(RtspSourceId) ?? throw new ArgumentNullException(nameof(RtspSourceId));
                node.SubscribeForDecodedFrame(UpdateFrame);
                node.SubscribeTrackingCompleted(UpdateResultFrame);
                node.SubscribeForMessages(UpdateMessage);

                DesktopApplicationContext.GetMessageBus().Subscribe<UpdateNodeResponse>(_id, (response) =>
                {
                    if (response.IsSuccess())
                    {
                        Dispatcher.UIThread.Post(() => { _dialog?.Close(); });
                    }
                    if (_currentEditViewModel != null)
                    {
                        response.FailureMessages.ForEach(message =>
                        {
                            _currentEditViewModel.Message += $"{message}\n";
                        });
                    }
                });
            }
        }
        public void UpdateMessage(object? sender,string message)
        {
            RtspSourceMessage = message;
        }
    

        public void UpdateResultFrame(object? sender,TrackedInfo trackedInfo)
        {
            lock (_trackingLock) {
                if (trackedInfo != null)
                {
                    if (trackedInfo.Image != null)
                    {
                        if (
                            (int)ResultImage.Size.Width != trackedInfo.Image.GetSize().Width ||
                            (int)ResultImage.Size.Height != trackedInfo.Image.GetSize().Height
                        )
                        {
                            _resultImageTemp1?.Dispose();
                            _resultImageTemp2?.Dispose();
                            _resultImageTemp1 = CreateBitmap(trackedInfo.Image.GetSize().Width, trackedInfo.Image.GetSize().Height);
                            _resultImageTemp2 = CreateBitmap(trackedInfo.Image.GetSize().Width, trackedInfo.Image.GetSize().Height);
                        }
                    }
                    if (trackedInfo.Image != null && !trackedInfo.Image.IsDisposedOrEmpty())
                    {
                        _resultImageSelector = !_resultImageSelector;
                        var temp = _resultImageSelector ? _resultImageTemp1 : _resultImageTemp2;
                        temp.WriteImageToUiComponent(trackedInfo.Image, _uiPixelFormat);
                        ResultImage = temp;

                    }
                    TrackedInfoResult = new TrackedInfoResultViewModel(trackedInfo);
                    if (Results.Count == 5)
                    {
                        Results[4].Dispose();
                        Results.RemoveAt(4);
                    }
                    Results.Insert(0, new TrackedInfoResultViewModel(trackedInfo, ResultImage.GetRoi(trackedInfo.PlateRectangle)));

                }
            }
            
        }
        public void UpdateFrame(object? sender, IDecodedFrame decodedVideoFrame)
        {
            if (decodedVideoFrame == null || CurrentFrame == null)
                return;
            try
            {
                if (VideoIsShowing)
                {
                    lock (_frameLock)
                    {

                        if ((int)CurrentFrame.Size.Width != decodedVideoFrame.GetSize().Width || (int)CurrentFrame.Size.Height != decodedVideoFrame.GetSize().Height)
                        {
                            _currentFrameTemp1?.Dispose();
                            _currentFrameTemp2?.Dispose();

                            _currentFrameTemp1 = CreateBitmap(decodedVideoFrame.GetSize().Width, decodedVideoFrame.GetSize().Height);
                            _currentFrameTemp2 = CreateBitmap(decodedVideoFrame.GetSize().Width, decodedVideoFrame.GetSize().Height);
                        }

                        _currentFrameSelecting = !_currentFrameSelecting;
                        var temp = _currentFrameSelecting ? _currentFrameTemp1 : _currentFrameTemp2;
                        temp.WriteImageToUiComponent(decodedVideoFrame, _uiPixelFormat);
                        CurrentFrame = temp;
                    }
                }
                _videoFrameEvent.Invoke(this, CurrentFrame);

            }
            catch (AccessViolationException ex)
            {
                DesktopApplicationContext?.GetLogger().LogError($"CameraViewModel Access violation in UpdateFrame: {ex.Message}");
            }
            catch (Exception ex)
            {
                DesktopApplicationContext?.GetLogger().LogError($"CameraViewModel Error in UpdateFrame: {ex.Message}"); ;
            }
        }
        [RelayCommand]
        public async Task EditLine()
        {
            await Task.Delay(10);
            var node = DesktopApplicationContext?.GetNodeManagerService().GetRtspNode(RtspSourceId);
            if (CurrentWindow == null || node == null)
                return;
            var sourceInfo = node.GetRtspSourceInfo();
            var sourceInfoModel = RtspSourceInfoModel.FromModel(sourceInfo);
            _currentEditViewModel = new CameraEditViewModel(UpdateSourceAsync, sourceInfoModel);
            _dialog = CameraEditView.CreateViewForUpdateDialog(
                sourceInfo.RtspConnectionInfo?.IpAddress ?? string.Empty,
                _currentEditViewModel
            );
            _videoFrameEvent += _currentEditViewModel.UpdateFrame;
            void OnDialogClosed(object? sender, EventArgs e)
            {
                _videoFrameEvent -= _currentEditViewModel.UpdateFrame;
                _dialog.Closed -= OnDialogClosed;
                _currentEditViewModel = null;
            }
            _dialog.Closed += OnDialogClosed;
            _ = _dialog.ShowDialog(CurrentWindow);


        }
        public async Task UpdateSourceAsync(RtspSourceInfoModel model)
        {
           await Task.Delay(10);
           if(DesktopApplicationContext != null)
            {
                var m = model.ToModel();
                var node = DesktopApplicationContext.GetNodeManagerService().GetRtspNode(RtspSourceId) ?? throw new ArgumentNullException(nameof(RtspSourceId));
                m.Id = node.GetRtspSourceInfo().Id;
                DesktopApplicationContext.GetMessageBus().Publish(new UpdateNodeRequest()
                {
                    RtspSourceInfo = m,
                });
            }
        }
        [RelayCommand]
        public void VideoShowing()
        {
            VideoIsShowing = !VideoIsShowing; 
        }
        [RelayCommand]
        public void Remove()
        {

            DesktopApplicationContext?.GetMessageBus().Publish(new DeleteNodeRequest() { SourceId = RtspSourceId });

        }
     

        private static WriteableBitmap CreateBitmap(int width, int height)
        {
            return new WriteableBitmap(
                new PixelSize(width, height),
                new Vector(96, 96),
                Avalonia.Platform.PixelFormat.Bgra8888,
                AlphaFormat.Opaque
            );
        }
    }
}
