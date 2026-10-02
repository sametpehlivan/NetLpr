using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Values.Request;

namespace NetLpr.Core.Values.Response
{
    public class DeleteNodeResponse : BaseResponse<DeleteNodeRequest>
    {
        public DeleteNodeResponse(DeleteNodeRequest request) : base(request)
        {
        }
    }
}
