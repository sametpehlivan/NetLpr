using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLpr.Core.Localization
{
    public interface ILocalizationService 
    {
        public (bool exists, string message) CheckLogMessage(string message, params object[] args);
    }
}
