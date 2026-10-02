using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLpr.Localization
{
    public interface ICultureProviderFactory
    {
        public ICultureProvider? GetCultureProvider();
    }
}
