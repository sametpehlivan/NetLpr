
using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using NetLpr.Core.Services;
using NetLpr.Desktop.ViewModels;
using NetLpr.Desktop.Views;

namespace NetLpr.Desktop
{
    public partial class App : Application
    {
        public static IDesktopApplicationContext? ApplicationContext { get; set; }
        public static MainWindow? MainWindow { get; private set; }
        public static MainWindowViewModel? MainWindowViewModel { get; set; }
        public override void Initialize()
        {
           
            AvaloniaXamlLoader.Load(this);
            var mainWindowViewModel = new MainWindowViewModel();
            MainWindowViewModel = mainWindowViewModel;
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {

                var initiatingWindow = new InitiatingWindow();
                initiatingWindow.Show();

                await Initiating(desktop);

                var mainWindow = new MainWindow
                {
                    DataContext = MainWindowViewModel
                };

                MainWindow = mainWindow;

                initiatingWindow.Closed += (_, _) =>
                {
                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();
                };

                initiatingWindow.Close();


            }
            
            base.OnFrameworkInitializationCompleted();
         
        }
        private async Task Initiating(IClassicDesktopStyleApplicationLifetime desktop)
        {
        

            await Task.Delay(5000);
        }
    }
}