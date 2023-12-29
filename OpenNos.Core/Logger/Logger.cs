using System;

namespace OpenNos.Core.Logger
{
    public class Logger
    {
        public static ISerilogLogger Log { get; set; }

        public static void InitializeLogger(ISerilogLogger log)
        {
            Log = log;
        }

        public static void Log(string v)
        {
            throw new NotImplementedException();
        }

        public static void LogError(string v, Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
