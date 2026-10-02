using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLpr.Core.Localization
{
    public enum MessageLevel
    {
        Trace, Debug, Info, Warning, Error, Critical
    }

    public class LocalizationMessage
    {
        public string Message { get; }
        public object[] Args { get; }
        public MessageLevel MessageLevel { get; set; }
        public LocalizationMessage(string message, params object[] args)
        {
            Message = message;
            Args = args;
            MessageLevel = MessageLevel.Info;
        }
    }
}
