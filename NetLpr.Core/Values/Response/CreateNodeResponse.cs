using NetLpr.Core.Services.Node;
using NetLpr.Core.Values.Request;

namespace NetLpr.Core.Values.Response
{
    public class CreateNodeResponse : BaseResponse<CreateNodeRequest> 
    {
        public RtspNode? RtspNode { get; private set; }
        public CreateNodeResponse(CreateNodeRequest request) : base(request) 
        {
        }
        public CreateNodeResponse(CreateNodeRequest request, RtspNode rtspNode) : base(request)
        {
            RtspNode = rtspNode;
        }
    }
}
