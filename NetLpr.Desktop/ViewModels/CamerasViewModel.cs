
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using NetLpr.Core.Values.Response;
using NetLpr.Desktop.Models;
using NetLpr.Desktop.Views;
using CommunityToolkit.Mvvm.Input;
using NetLpr.Core.Values.Request;

namespace NetLpr.Desktop.ViewModels
{
    public partial class CamerasViewModel : ViewModelBase
    {
       
        private string _id = Guid.NewGuid().ToString();
        private CameraEditViewModel? _currentEditViewModel;
        private Window? _dialog;

        public ObservableCollection<CameraViewModel> Cameras { get; set; } = new();
        public CamerasViewModel()
        {
            if(DesktopApplicationContext != null)
            {
                DesktopApplicationContext.GetNodeManagerService()
                 .GetAllRtspSourceInfos()
                 .ForEach(x =>
                 {
                     var cameraViewModel = new CameraViewModel(x.Id);
                     Cameras.Add(cameraViewModel);
                 });
                DesktopApplicationContext
                    .GetMessageBus()
                    .Subscribe<CreateNodeResponse>(_id, (response) =>
                    {
                        if (response.IsSuccess() && response.RtspNode != null)
                        {
                            var cameraViewModel = new CameraViewModel(response.RtspNode.GetRtspSourceInfo().Id);
                            Cameras.Add(cameraViewModel);
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
                DesktopApplicationContext
                    .GetMessageBus()
                    .Subscribe<DeleteNodeResponse>(_id, (response) =>
                    {
                        if (response.IsSuccess())
                        {

                            for (int i = 0; i < Cameras.Count; i++)
                            {
                                var cameraViewModel = Cameras[i];
                                if (cameraViewModel.RtspSourceId.Equals(response.Request.SourceId))
                                {
                                    Cameras.Remove(cameraViewModel);
                                }
                            }
                        }

                    });
            }
            
        }
        ~CamerasViewModel()
        {
           if(DesktopApplicationContext != null)
            {
                DesktopApplicationContext
                .GetMessageBus()
                .Unsubscribe<CreateNodeResponse>(_id);
                DesktopApplicationContext
                .GetMessageBus()
                .Unsubscribe<DeleteNodeResponse>(_id);
            }
        }
        [RelayCommand]
        public void ShowCreateOrEditCameraDialog()
        {
            if (CurrentWindow == null)
                return;
            _currentEditViewModel = new CameraEditViewModel(OnCreating);
            _dialog = CameraEditView.CreateViewForCreateDialog(Labels.AddNewCamera, _currentEditViewModel);
            _dialog.ShowDialog(CurrentWindow);
        }
        public async Task OnCreating(RtspSourceInfoModel model)
        {
            await Task.Delay(10);

            var request = new CreateNodeRequest()
            {
                RtspSourceInfo = model.ToModel()
            };
            if(DesktopApplicationContext != null)
            {
                DesktopApplicationContext.GetMessageBus().Publish(request);
            }
           
        }

   
    }
}
