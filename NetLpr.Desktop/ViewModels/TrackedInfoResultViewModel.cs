using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media.Imaging;
using NetLpr.Core.Extensions;
using NetLpr.Core.Models;
using NetLpr.Core.Values.Tracking;
using NetLpr.Desktop.Extensions;
using NetLpr.Desktop.Models;

namespace NetLpr.Desktop.ViewModels
{
    public partial class TrackedInfoResultViewModel : ViewModelBase 
    {
        public bool IsRow { get; set; } = true;
        public string ResultIpAddress { get; set; } = string.Empty;
        public string ResultPlate { get; set; } = string.Empty;
        public string ResultDate { get; set; } = string.Empty;
        public string ResultCarType { get; set; } = string.Empty;
        public string ResultRegion { get; set; } = string.Empty;
        public string ResultDirection { get; set; } = string.Empty;
        public bool IsUp { get; set; } = false;
        public WriteableBitmap? Image { get; set; }
        public TrackedInfoResultViewModel()
        {

        }
        public TrackedInfoResultViewModel(TrackedInfo trackedInfo)
        {

            Initialize(trackedInfo);
        }
        public TrackedInfoResultViewModel(TrackedInfo trackedInfo , WriteableBitmap image)
        {
            Initialize(trackedInfo,false,image);
        }
        private void Initialize(TrackedInfo trackedInfo, bool isRow = true, WriteableBitmap? image = null)
        {
            IsRow = isRow;
            Image = image;
            var node = DesktopApplicationContext?.GetNodeManagerService().GetRtspNode(trackedInfo.SourceId);

            var responses = trackedInfo.TrackingValues;
            responses.TryGetValue(TrackedInfo.PLATES, out List<ScoreAndValue>? plates);
            responses.TryGetValue(TrackedInfo.CAR_TYPES, out List<ScoreAndValue>? carTypes);
            responses.TryGetValue(TrackedInfo.REGIONS, out List<ScoreAndValue>? regions);
            responses.TryGetValue(TrackedInfo.DIRECTION, out List<ScoreAndValue>? directions);

            if (node != null)
            {
                ResultIpAddress = node.GetRtspSourceInfo().RtspConnectionInfo.IpAddress;
            }
            else
                ResultIpAddress = "-";
            if (plates == null || plates.Count == 0)
                ResultPlate = "-";
            else
                ResultPlate = plates.OrderByDescending(x => x.Score).First().Value.Replace("_", "");
            if (carTypes == null || carTypes.Count == 0)
                ResultCarType = "-";
            else
                ResultCarType = LocalizationService?.GetLocalizeMessage(carTypes.OrderByDescending(x => x.Score).First().Value) ?? string.Empty;
            ResultDate = trackedInfo.DateTime.ToString("d MMMM yyyy HH:mm:ss", DesktopApplicationContext?.GetCultureInfo()) ?? string.Empty;
            if (regions == null || regions.Count == 0)
                ResultRegion = "-";
            else
                ResultRegion = LocalizationService?.GetLocalizeMessage(regions.OrderByDescending(x => x.Score).First().Value) ?? string.Empty;
            if (directions == null || directions.Count != 2)
                ResultDirection = "-";
            else
            {
                IsUp = !(directions[1].Score > 0);
                ResultDirection = IsUp
                    ? LocalizationService?.GetLocalizeMessage("direction.up") ?? string.Empty
                    : LocalizationService?.GetLocalizeMessage("direction.down") ?? string.Empty;
            }
        }
        public void Dispose()
        {

            Image?.Dispose();
        }
    }
}
