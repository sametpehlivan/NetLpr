using System.Drawing;
using NetLpr.Core.Exceptions;
using NetLpr.Core.Validators.Sources;


namespace NetLpr.Core.Models
{
    public enum RtspSourceStatus
    {
        INITIALIZED,
        CONNECTING,
        CONNECTED,
        STARTING,
        STARTED,
        DISCONNECTED,
        CLOSING,
        CLOSED
    }
    public enum RtspTransportType
    {
        TCP,
        UDP
    }
    public record  RtspConnectionInfo
    {
        public static readonly RtspConnectionInfo Empty = new RtspConnectionInfo();
        public string IpAddress { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PathAndQuery { get; set; } = string.Empty;
        public RtspTransportType TransportType { get; set; }

        public List<string> Validate()
        {
            return RtspConnectionInfoValidator.Validate(this);
        }


        public string GetRtspUrl()
        {

            var port = string.IsNullOrEmpty(Port) ? "554" : Port;
            string credentials = string.Empty;
            if (!string.IsNullOrEmpty(Username))
            {
                credentials = string.IsNullOrEmpty(Password)
                    ? $"{Username}@"
                    : $"{Username}:{Password}@";
            }
            string path = string.IsNullOrEmpty(PathAndQuery) ?
                string.Empty :
                
                    PathAndQuery.StartsWith('/') ?
                    PathAndQuery :
                    $"/{PathAndQuery}"
                ;
            return $"rtsp://{credentials}{IpAddress}:{port}{path}";
        }
    }

    public class RtspSourceInfo 
    {
        public static readonly RtspSourceInfo Empty = new RtspSourceInfo();
        public string Id { get; set; } = string.Empty;
        public RtspConnectionInfo RtspConnectionInfo { get; set; } = RtspConnectionInfo.Empty;
        public StreamAnalysesInfo StreamAnalysesInfo { get; set; } = new();
        public RtspSourceStatus RtspSourceStatus { get; set; } = RtspSourceStatus.INITIALIZED;
        public void ValidateConnection()
        {
            var errors =  RtspConnectionInfo.Validate();
            if (errors.Count() > 0)
                throw new LocalizationException(errors);
        }
        public void Validate()
        {
            var errors = RtspConnectionInfo.Validate();
            errors.AddRange(StreamAnalysesInfo.Validate());
            if (errors.Count() > 0)
                throw new LocalizationException(errors);
        }
    }
}
