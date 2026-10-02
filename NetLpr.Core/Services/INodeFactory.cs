using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Values.Request;
using NetLpr.Core.Values.Response;

namespace NetLpr.Core.Services
{
    public interface INodeFactory 
    {
        public CreateNodeResponse CreateNode(CreateNodeRequest request);
    }
}
