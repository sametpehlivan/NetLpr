using NetLpr.Core.Models;
using NetLpr.Core.Values.Inference;

namespace NetLpr.Core.Values.Request
{
    public class CreateNodeRequest : BaseRequest
    {
        public RtspSourceInfo RtspSourceInfo { get; set; } = new();
        public void Validate()
        {
            RtspSourceInfo.ValidateConnection();
        }
    }
}
