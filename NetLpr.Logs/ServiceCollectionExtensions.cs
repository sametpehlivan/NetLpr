using NetLpr.Core.Log;
using Microsoft.Extensions.DependencyInjection;
using NetLpr.Logs;
using Serilog;

namespace NetLpr.Logs
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSerilogAdapter(this IServiceCollection services,SerilogConfigs configs)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose() 
            .Enrich.FromLogContext()
            .WriteTo.Console()      
            .WriteTo.File(
                path: $"{configs.BasePath}/log-.txt",                    
                rollingInterval: RollingInterval.Day,       
                retainedFileCountLimit: 15,                
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
            services.AddScoped<Core.Log.ILogger, FileLogger>();
            return services;
        } 
    }
}
