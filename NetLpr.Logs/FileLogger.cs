using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Log;

namespace NetLpr.Logs
{
    public class FileLogger : ILogger
    {
        public FileLogger()
        {

        }
        private void WriteLog(Serilog.Events.LogEventLevel level, Exception? exception, string message, params object[] args)
        {
            
            if (exception != null)
            {
                Serilog.Log.Write(level, exception, message);
            }
            else
            {
                Serilog.Log.Write(level, message);
            }
        }


        public void LogTrace(string message, params object[] args) => WriteLog(Serilog.Events.LogEventLevel.Verbose, null, message, args);
        public void LogTrace(Exception exception, string message, params object[] args) => WriteLog(Serilog.Events.LogEventLevel.Verbose, exception, message, args);

        public void LogDebug(string message, params object[] args) => WriteLog(Serilog.Events.LogEventLevel.Debug, null, message, args);
        public void LogDebug(Exception exception, string message, params object[] args) => WriteLog(Serilog.Events.LogEventLevel.Debug, exception, message, args);

        public void LogInformation(string message, params object[] args) => WriteLog(Serilog.Events.LogEventLevel.Information, null, message, args);
        public void LogInformation(Exception exception, string message, params object[] args) => WriteLog(Serilog.Events.LogEventLevel.Information, exception, message, args);

        public void LogWarning(string message, params object[] args) => WriteLog(Serilog.Events.LogEventLevel.Warning, null, message, args);
        public void LogWarning(Exception exception, string message, params object[] args) => WriteLog(Serilog.Events.LogEventLevel.Warning, exception, message, args);

        public void LogError(string message, params object[] args) => WriteLog(Serilog.Events.LogEventLevel.Error, null, message, args);
        public void LogError(Exception exception, string message, params object[] args) => WriteLog(Serilog.Events.LogEventLevel.Error, exception, message, args);

        public void LogCritical(string message, params object[] args) => WriteLog(Serilog.Events.LogEventLevel.Fatal, null, message, args);
        public void LogCritical(Exception exception, string message, params object[] args) => WriteLog(Serilog.Events.LogEventLevel.Fatal, exception, message, args);
    }
}
