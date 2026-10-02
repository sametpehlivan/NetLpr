
using System.Globalization;
using NetLpr.Localization;
using OpenCvSharp;

namespace NetLpr.Boot.Loalization   
{
    public class CultureProvider : ICultureProvider
    {
        private CultureInfo _currentCulture = CultureInfo.InstalledUICulture;

        public CultureInfo CurrentCulture => _currentCulture;
        public void SetCulture(string cultureName)
        {
            _currentCulture = new CultureInfo(cultureName);
            CultureInfo.CurrentCulture = _currentCulture;
            CultureInfo.CurrentUICulture = _currentCulture;
            CultureInfo.DefaultThreadCurrentCulture = _currentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = _currentCulture;

        }
    }
}
