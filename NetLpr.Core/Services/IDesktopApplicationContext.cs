using System.Globalization;
using NetLpr.Core.Localization;
using NetLpr.Core.Log;
using NetLpr.Core.Services.Messaging;

namespace NetLpr.Core.Services
{
    public interface IDesktopApplicationContext 
    {
        public ILogger GetLogger();
        public ILocalizationService GetLocalizationService();
        public void SubscribeCultureInfo(EventHandler<CultureInfo> action);
        public void UnsubscribeCultureInfo(EventHandler<CultureInfo> action);
        public void ChangeCultureInfo(string culture);
        public CultureInfo GetCultureInfo();
        public IMessageBus GetMessageBus();
        public NodeManagerService GetNodeManagerService();
        public TrackedInfoManagerService GetTrackedInfoManagerService();
    } 
}
