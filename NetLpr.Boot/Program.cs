using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using NetLpr.Persistence.Sqlite;
using NetLpr.Boot.Loalization;
using NetLpr.Boot.Node;
using NetLpr.Core.Helpers;
using NetLpr.Core.Services;
using NetLpr.Core.Services.Messaging;
using NetLpr.Core.Services.Sources;
using NetLpr.Core.Services.Syncronizations;
using NetLpr.Core.Services.Tracking;
using NetLpr.Desktop;
using NetLpr.InferenceOR;
using NetLpr.InferenceOV;
using NetLpr.Localization;
using NetLpr.Logs;
using NetLpr.RealTimeStream.Ffmpeg;
using Microsoft.Extensions.DependencyInjection;


namespace NetLpr.Boot
{
    public class Program
    {
        private static readonly IServiceCollection _service = new ServiceCollection();
        private static IServiceProvider _serviceProvider;
        private static Mutex? _mutex;

        public static void Main(string[] args)
        {   
            try
            {

                _mutex = new Mutex(true, "net-lpr", out bool createdNew);
                if (!createdNew)
                {
                    Console.WriteLine("Application already running!");
                    Console.ReadLine();
                }
                else
                {
                    _service.AddSingleton<ICultureProvider, CultureProvider>();
                    _service.AddLocalization(new LocalizationConfig() { BasePath = "./locals" });
                    _service.AddSerilogAdapter(new SerilogConfigs() { BasePath = "./logs" });
                    _service.AddFfmpegAdapter(new() { BasePath = "./ffmpeg_libs" });
                    _service.AddSingleton<IMessageBus, InMemoryMessageBus>();
                    _service.AddSqliteAdapter();
                    _service.AddSingleton<RtspSourceContext>();
                    _service.AddSingleton<SynchronizationServiceContext>();
                    _service.AddSingleton<TrackingServiceContext>();
                    _service.AddSingleton<OpenVinoInferencePoolFactory>();
                    _service.AddSingleton<OnnxModelFactory>();
                    _service.AddSingleton<INodeFactory, NodeFactory>();
                    _service.AddSingleton<NodeManagerService>();
                    _service.AddSingleton<TrackedInfoManagerService>();
                    _service.AddSingleton<IDesktopApplicationContext, ApplicationContext>();
                    _serviceProvider = _service.BuildServiceProvider();
                    _serviceProvider.MigrateDatabase();
                    _ = StartDesktop(args);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }
        public static async Task StartDesktop(string[] args)
        {
            App.ApplicationContext = _serviceProvider.GetRequiredService<IDesktopApplicationContext>();

            var avaloniaThread = new Thread(() =>
            {
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            });

            avaloniaThread?.SetApartmentState(ApartmentState.MTA);
            avaloniaThread?.Start();
        }
        public static AppBuilder BuildAvaloniaApp()
             => AppBuilder.Configure<App>()
                 .UsePlatformDetect()
                 .WithInterFont()
                 .LogToTrace();

    }
}
