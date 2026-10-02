using Avalonia.Controls;
using NetLpr.Core.Localization;
using NetLpr.Core.Log;
using NetLpr.Core.Services;
using NetLpr.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NetLpr.Desktop.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        public IDesktopApplicationContext? DesktopApplicationContext => App.ApplicationContext;
        public ILocalizationService? LocalizationService => DesktopApplicationContext?.GetLocalizationService();
        public ILogger? Logger => DesktopApplicationContext?.GetLogger();
        public Window? CurrentWindow => App.MainWindow;
        public Labels Labels => Labels.Instance;
        public ViewModelBase()
        {

        }
    }
}
