using System.Globalization;
using NetLpr.Core.Extensions;
using NetLpr.Desktop.Extensions;


namespace NetLpr.Desktop.Models
{
    public class Labels : CultureListenerReactiveObject
    {
        public static Labels Instance = new Labels();

        public string RtspConnection => LocalizationService?.GetLocalizeMessage("label.rtspConnection") ?? string.Empty;
        public string IpAddress => LocalizationService?.GetLocalizeMessage("label.ipAddress") ?? string.Empty;
        public string Port => LocalizationService?.GetLocalizeMessage("label.port") ?? string.Empty;
        public string Username => LocalizationService?.GetLocalizeMessage("label.username") ?? string.Empty;
        public string Password => LocalizationService?.GetLocalizeMessage("label.password") ?? string.Empty;
        public string PathAndQuery => LocalizationService?.GetLocalizeMessage("label.pathAndQuery") ?? string.Empty;
        public string TransportType => LocalizationService?.GetLocalizeMessage("label.transportType") ?? string.Empty;
        public string TargetSize => LocalizationService?.GetLocalizeMessage("label.targetSize") ?? string.Empty;
        public string Width => LocalizationService?.GetLocalizeMessage("label.width") ?? string.Empty;
        public string Height => LocalizationService?.GetLocalizeMessage("label.height") ?? string.Empty;
        public string AnalysesInfo => LocalizationService?.GetLocalizeMessage("label.analysesInfo") ?? string.Empty;
        public string AnalyseType => LocalizationService?.GetLocalizeMessage("label.analyseType") ?? string.Empty;
        public string Save => LocalizationService?.GetLocalizeMessage("label.save") ?? string.Empty;
        public string VehicleDetectionModel => LocalizationService?.GetLocalizeMessage("label.vehicleDetectionModel") ?? string.Empty;
        public string PlateDetectionModel => LocalizationService?.GetLocalizeMessage("label.plateDetectionModel") ?? string.Empty;
        public string PlateOcrModel => LocalizationService?.GetLocalizeMessage("label.plateOcrModel") ?? string.Empty;
        public string AddNewCamera => LocalizationService?.GetLocalizeMessage("desktop.cameraList.addNewCamera") ?? string.Empty;

        public string Results => LocalizationService?.GetLocalizeMessage("label.results") ?? string.Empty;
        public string ResultIpAddress => LocalizationService?.GetLocalizeMessage("label.result.ipAddress") ?? string.Empty;
        public string ResultPlate => LocalizationService?.GetLocalizeMessage("label.result.plate") ?? string.Empty;
        public string ResultDate => LocalizationService?.GetLocalizeMessage("label.result.date") ?? string.Empty;
        public string ResultRegion => LocalizationService?.GetLocalizeMessage("label.result.region") ?? string.Empty;
        public string ResultCarType => LocalizationService?.GetLocalizeMessage("label.result.vehicleType") ?? string.Empty;
        public string ResultDirection => LocalizationService?.GetLocalizeMessage("label.result.direction") ?? string.Empty;

        public string SelectRoi => LocalizationService?.GetLocalizeMessage("label.selectRoi") ?? string.Empty;
        public string SelectArea => LocalizationService?.GetLocalizeMessage("label.selectArea") ?? string.Empty;
        public string IsAnalysesOpen => LocalizationService?.GetLocalizeMessage("labels.analysesOpen") ?? string.Empty;

        public string Cameras => LocalizationService?.GetLocalizeMessage("desktop.sidebar.cameras") ?? string.Empty;
        public string CamerasManageInfo => LocalizationService?.GetLocalizeMessage("labels.cameras.manageInfo") ?? string.Empty;
        public Labels() : base() {
            NameOfMultiLangProperties = new[] {
                nameof(AddNewCamera),
                nameof(ResultIpAddress),
                nameof(ResultPlate),
                nameof(ResultDate),
                nameof(ResultRegion),
                nameof(ResultCarType),
                nameof(ResultDirection),
                nameof(Cameras),
                nameof(CamerasManageInfo),
            };
        }

    }
}
