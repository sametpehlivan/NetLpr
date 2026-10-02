using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NetLpr.Desktop.Models
{
    public partial class RtspConnectionInfoModel : ObservableObject
    {
        [ObservableProperty]
        private string _ipAddress = string.Empty;
        [ObservableProperty]
        private string _port = string.Empty;
        [ObservableProperty]
        private string _username = string.Empty;
        [ObservableProperty]
        private string _password = string.Empty;
        [ObservableProperty]
        private string _pathAndQuery = string.Empty;
        [ObservableProperty]
        private RtspTransportType _transportType;

        public static RtspConnectionInfoModel FromModel(RtspConnectionInfo model)
        {
            return new RtspConnectionInfoModel
            {
                IpAddress = model.IpAddress,
                Port = model.Port,
                Username = model.Username,
                Password = model.Password,
                PathAndQuery = model.PathAndQuery,
                TransportType = model.TransportType,
            };
        }
        public RtspConnectionInfo ToModel()
        {
            return new RtspConnectionInfo
            {
                IpAddress = IpAddress,
                Port = Port,
                Username = Username,
                Password = Password,
                PathAndQuery = PathAndQuery,
                TransportType = TransportType,
            };
        }

    }
}
