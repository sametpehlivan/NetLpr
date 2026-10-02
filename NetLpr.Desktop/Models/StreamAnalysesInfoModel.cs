using System.Collections.ObjectModel;
using System.Linq;
using NetLpr.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;


namespace NetLpr.Desktop.Models
{
    public partial class StreamAnalysesInfoModel : ObservableObject
    {
        [ObservableProperty]
        private StreamAnalysesType _analysesType;
        [ObservableProperty]
        private RectangleModel _analysesRoi = new RectangleModel { X = 0, Y = 0, Width = 1, Height = 1 };
        [ObservableProperty]
        private bool _isAnalysesOpen = false;
        public ObservableCollection<PointModel> AnalysesPoints { get; } = new ObservableCollection<PointModel>();
        public StreamAnalysesInfoModel()
        {

        }
        public static StreamAnalysesInfoModel FromModel(StreamAnalysesInfo analysesInfo)
        {
            var model = new StreamAnalysesInfoModel
            {
                IsAnalysesOpen = analysesInfo.IsAnalysesOpen,
                AnalysesType = analysesInfo.AnalysesType,
                AnalysesRoi = RectangleModel.FromModel(analysesInfo.AnalysesRoi)
            };
            analysesInfo.AnalysesPoints.ForEach(x => model.AnalysesPoints.Add(PointModel.FromModel(x)));
            return model;
        }
        public StreamAnalysesInfo ToModel()
        {
            return new StreamAnalysesInfo
            {
                IsAnalysesOpen = IsAnalysesOpen,
                AnalysesType = AnalysesType,
                AnalysesRoi = AnalysesRoi.ToModel(),
                AnalysesPoints = AnalysesPoints.Select(x => x.ToModel()).ToList()
            };
        }
    }
}
