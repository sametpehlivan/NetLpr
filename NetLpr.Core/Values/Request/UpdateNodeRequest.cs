using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Models;

namespace NetLpr.Core.Values.Request
{
    public class UpdateNodeRequest : BaseRequest
    {
        public RtspSourceInfo RtspSourceInfo { get; set; } = new();
        public void Validate()
        {
            RtspSourceInfo.Validate();
        }
    }
}
