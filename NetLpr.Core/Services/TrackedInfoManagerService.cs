using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Models;
using NetLpr.Core.Services.Messaging;
using NetLpr.Core.Values.Request;

namespace NetLpr.Core.Services
{
    public class TrackedInfoManagerService
    {
        private ITrackedInfoRepository _trackedInfoRepository;
        private IMessageBus _messageBus;
        public TrackedInfoManagerService(IMessageBus messageBus,ITrackedInfoRepository trackedInfoRepository) 
        {
            _messageBus = messageBus;
            _trackedInfoRepository = trackedInfoRepository;
        }

    }
}
