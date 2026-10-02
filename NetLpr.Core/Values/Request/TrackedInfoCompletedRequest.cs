using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Models;
using NetLpr.Core.Services;

namespace NetLpr.Core.Values.Request
{
    public class TrackedInfoCompletedRequest : BaseRequest
    {
        public TrackedInfo? TrackedInfo { get; set; }
    }
}
