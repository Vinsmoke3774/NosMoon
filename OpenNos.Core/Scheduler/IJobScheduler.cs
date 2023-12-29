using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core.Scheduler
{
    public interface IJobScheduler
    {
        /// <summary>
        /// Deletes a job
        /// </summary>
        /// <param name="jobId"></param>
        void Delete(string jobId);

        /// <summary>
        /// Deletes a job
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="fromState"></param>
        void Delete(string jobId, string fromState);
    }

}
