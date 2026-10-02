using System.Globalization;
using NetLpr.Core.Localization;
using NetLpr.Core.Log;
using NetLpr.Core.Services;
using NetLpr.Core.Services.Messaging;
using NetLpr.Localization;


namespace NetLpr.Boot
{
    public class ApplicationContext : IDesktopApplicationContext
    {
        private ILogger _logger;
        private ICultureProvider _cultureProvider;
        private EventHandler<CultureInfo>? _cultureInfoChanged;
        private IMessageBus _messageBus;
        private ILocalizationService _localizationService;
        private NodeManagerService _nodeManagerService;
        private TrackedInfoManagerService _trackingInfoManagerService;
        public ApplicationContext(
            ILogger logger,
            ILocalizationService localizationService,
            ICultureProvider cultureProvider,
            IMessageBus messageBus,
            NodeManagerService nodeManagerService,
            TrackedInfoManagerService trackingInfoManagerService
        )
        {
            _nodeManagerService = nodeManagerService;
            _trackingInfoManagerService = trackingInfoManagerService;
            _logger = logger;
            _cultureProvider = cultureProvider;
            _messageBus = messageBus;
            _localizationService = localizationService;

        }
        public ILogger GetLogger()
        {
            return _logger;
        }

        public ILocalizationService GetLocalizationService()
        {
            return _localizationService;
        }
        public void ChangeCultureInfo(string culture)
        {

            _cultureProvider.SetCulture(culture);
            _cultureInfoChanged?.Invoke(this, _cultureProvider.CurrentCulture);
        }

        public void SubscribeCultureInfo(EventHandler<CultureInfo> action)
        {
            if (action == null) return;
            _cultureInfoChanged += action;
        }

        public void UnsubscribeCultureInfo(EventHandler<CultureInfo> action)
        {
            if (action == null) return;
            _cultureInfoChanged -= action;
        }

        public CultureInfo GetCultureInfo()
        {
            return _cultureProvider.CurrentCulture;
        }

        public IMessageBus GetMessageBus()
        {
           return _messageBus;
        }

        public NodeManagerService GetNodeManagerService()
        {
            return _nodeManagerService;
        }

        public TrackedInfoManagerService GetTrackedInfoManagerService()
        {
            return _trackingInfoManagerService;
        }
    }
}
