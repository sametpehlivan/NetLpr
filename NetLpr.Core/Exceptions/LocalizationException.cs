using NetLpr.Core.Localization;

namespace NetLpr.Core.Exceptions
{
    public class LocalizationException : Exception
    {
        
        public List<LocalizationMessage> MessageKeys { get; }

        public LocalizationException(LocalizationMessage messageKey)
        {
            MessageKeys = new List<LocalizationMessage>() { messageKey };
        }
        public LocalizationException(List<LocalizationMessage> messageKeys) 
        {
            MessageKeys = messageKeys;
            MessageKeys.ForEach(x => x.MessageLevel = MessageLevel.Error);
        }
        public LocalizationException(List<string> keys)
        {
            MessageKeys = keys.Select(x => new LocalizationMessage(x) { MessageLevel = MessageLevel.Error }).ToList();
        }
        public static void ThrowIfNull(object? ob,string messageKey)
        {
            if (ob == null) throw new LocalizationException(new List<LocalizationMessage>() { new LocalizationMessage(messageKey) { MessageLevel = MessageLevel.Error } });
        }
    }
}
