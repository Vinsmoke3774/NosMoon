using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Logging.LogProviders;
using Hangfire.MemoryStorage;

namespace NosByte.Scheduler
{
    internal static class SchedulerBootstrap
    {
        private static BackgroundJobServer _server;

        public static bool ServerStarted { get; set; }

        public static void StartScheduler()
        {
            if (ServerStarted)
            {
                return;
            }

            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMemoryStorage();

            GlobalConfiguration.Configuration.UseLogProvider(new NoLoggingProvider());

            _server = new BackgroundJobServer();
            ServerStarted = true;
        }

        public static void StopScheduler()
        {
            _server.Dispose();
        }
    }

}
