using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLpr.Core.Values.Request
{
    public class DeleteNodeRequest : BaseRequest
    {
        public string SourceId{ get; set; } = string.Empty;
    }
}
