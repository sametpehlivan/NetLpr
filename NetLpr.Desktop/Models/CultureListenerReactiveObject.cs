using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Localization;
using NetLpr.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NetLpr.Desktop.Models
{
    public abstract class CultureListenerReactiveObject : ObservableObject
    {
        public IDesktopApplicationContext? DesktopApplicationContext => App.ApplicationContext;
        public ILocalizationService? LocalizationService => DesktopApplicationContext?.GetLocalizationService();
        public string[]? NameOfMultiLangProperties { get; protected set; }
        public CultureListenerReactiveObject()
        {

            DesktopApplicationContext?.SubscribeCultureInfo(CultureInfoChanged);
        }
        public void CultureInfoChanged(object? sender, CultureInfo info)
        {
            if (NameOfMultiLangProperties == null)
                return;
            foreach (var item in NameOfMultiLangProperties)
            {
                this.OnPropertyChanged(item);
            }
        }

        ~CultureListenerReactiveObject()
        {
            DesktopApplicationContext?.UnsubscribeCultureInfo(CultureInfoChanged);
        }

    }
}
