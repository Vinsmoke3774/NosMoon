using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Logging;

namespace NosByte.Scheduler
{
    class NoLoggingProvider : ILogProvider
    {
        public ILog GetLogger(string name)
        {
            return new NoLoggingLogger();
        }
    }

}
