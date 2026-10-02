using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Localization;
using NetLpr.Core.Log;

namespace NetLpr.Core.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogLocalizationMessage(this ILogger logger, ILocalizationService localizationService, LocalizationMessage message)
        {
            var newMessage = localizationService.CheckLogMessage(message.Message, message.Args);
            var loggedMessage = message.Message;
            if (newMessage.exists)
                loggedMessage = newMessage.message;
            if (MessageLevel.Trace == message.MessageLevel)
                logger.LogTrace(loggedMessage);
            else if (MessageLevel.Debug == message.MessageLevel)
                logger.LogDebug(loggedMessage);
            else if (MessageLevel.Info == message.MessageLevel)
                logger.LogInformation(loggedMessage);
            else if (MessageLevel.Warning == message.MessageLevel)
                logger.LogWarning(loggedMessage);
            else if (MessageLevel.Error == message.MessageLevel)
                logger.LogError(loggedMessage);
            else
                logger.LogInformation(loggedMessage);
        }
    }
}
