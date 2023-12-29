using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System;

namespace OpenNos.Core.Logger
{
    public class SerilogLogger : ISerilogLogger
    {
        private readonly ILogger _logger;

        public SerilogLogger()
        {
            _logger = new LoggerConfiguration()
# if (DEBUG)
                .MinimumLevel.Debug()
#elif (RELEASE)
                .MinimumLevel.Warning()
#endif
                .Enrich.WithThreadId()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
        }

        public void LogUserEvent(string logEvent, string caller, string data)
        {
            _logger.Information($"{logEvent} {caller} {data}");
        }

        public void Debug(string msg)
        {
            _logger.Debug(msg);
        }

        public void DebugFormat(string msg, params object[] objs)
        {
            _logger.Debug(msg, objs);
        }

        public void Info(string msg)
        {
            _logger.Information(msg);
        }

        public void InfoFormat(string msg, params object[] objs)
        {
            _logger.Information(msg, objs);
        }

        public void Warn(string msg)
        {
            _logger.Warning(msg);
        }

        public void WarnFormat(string msg, params object[] objs)
        {
            _logger.Warning(msg, objs);
        }

        public void Error(string msg, Exception ex)
        {
            _logger.Error(ex, msg);
        }

        public void ErrorFormat(string msg, Exception ex, params object[] objs)
        {
            _logger.Error(ex, msg, objs);
        }

        public void Fatal(string msg, Exception ex)
        {
            _logger.Fatal(ex, msg);
        }
    }
}
