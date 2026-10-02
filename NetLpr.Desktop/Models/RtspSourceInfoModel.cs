using System;
using NetLpr.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;


namespace NetLpr.Desktop.Models
{
    public partial class RtspSourceInfoModel : ObservableObject
    {
        public RtspTransportType[] RtspTransportTypes { get; } =  Enum.GetValues<RtspTransportType>();
        public StreamAnalysesType[] StreamAnalysesTypes { get; } = new StreamAnalysesType[] { StreamAnalysesType.TrackingPlate };
        
        [ObservableProperty]
        private string _id = string.Empty;

        [ObservableProperty]
        private RtspConnectionInfoModel _rtspConnectionInfo;

        [ObservableProperty]
        private SizeModel _frameTransformSize;

        [ObservableProperty]
        private StreamAnalysesInfoModel _streamAnalysesInfo;



        public RtspSourceInfoModel()
        {
            _rtspConnectionInfo = new();
            _streamAnalysesInfo = new();
            _frameTransformSize = new() { Width = 1920, Height = 1080 };
           
        }
        public static RtspSourceInfoModel FromModel(RtspSourceInfo model)
        {
            return new RtspSourceInfoModel
            {
                Id = model.Id,
                RtspConnectionInfo = RtspConnectionInfoModel.FromModel(model.RtspConnectionInfo),
                StreamAnalysesInfo = StreamAnalysesInfoModel.FromModel(model.StreamAnalysesInfo),
            };
        }
        public RtspSourceInfo ToModel()
        {
            return new RtspSourceInfo
            {
                Id = Id,
                RtspConnectionInfo = RtspConnectionInfo.ToModel(),
                StreamAnalysesInfo = StreamAnalysesInfo.ToModel(),
            };
        }
    }

}
