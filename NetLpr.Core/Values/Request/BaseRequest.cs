using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLpr.Core.Values.Request
{
    public abstract class BaseRequest 
    {

        private string _id = Guid.NewGuid().ToString();
        private DateTime _createdTime = DateTime.UtcNow;

        public DateTime GetCreatedDateTime()
        {
            return _createdTime;
        }

        public string GetRequestId()
        {
            return _id;
        }
    }
}
