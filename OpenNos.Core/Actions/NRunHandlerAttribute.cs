using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Domain;

namespace OpenNos.Core.Actions
{
    /// <summary>
    /// Attribute that defines a NRun packet for a class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NRunHandlerAttribute : Attribute
    {
        public NRunHandlerAttribute(NRunType[] runners)
        {
            Runners = runners;
        }

        public NRunHandlerAttribute(NRunType runner)
        {
            Runners = new[]
            {
                runner
            };
        }

        /// <summary>
        /// Different runners
        /// </summary>
        public NRunType[] Runners { get; set; }
    }

}
