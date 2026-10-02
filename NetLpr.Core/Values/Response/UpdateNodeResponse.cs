using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Models;
using NetLpr.Core.Values.Request;

namespace NetLpr.Core.Values.Response
{
    public class UpdateNodeResponse : BaseResponse<UpdateNodeRequest>
    {
        public RtspSourceInfo? RtspSourceInfo { get; set; }
        public UpdateNodeResponse(UpdateNodeRequest request) : base(request)
        {
        }
    }
}
