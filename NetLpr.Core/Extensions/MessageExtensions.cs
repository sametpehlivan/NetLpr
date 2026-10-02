using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Localization;

namespace NetLpr.Core.Extensions
{
    public static class MessageExtensions
    {
        public static string GetLocalizeMessageWithDefaultMessage(this ILocalizationService service,string messageKey,string defaultMessage , params object[] args)
        {
            var res = service.CheckLogMessage(messageKey, args);
            return res.exists ? res.message : defaultMessage;
        }
        public static string GetLocalizeMessage(this ILocalizationService service, string messageKey, params object[] args)
        {
            return service.GetLocalizeMessageWithDefaultMessage(messageKey, string.Empty, args);
        }
    }
}
