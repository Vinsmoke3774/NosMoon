using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Domain
{
    public enum CronEventType : byte
    {
        Minute,
        Hour,
        Daily,
        Weekly,
        Monthly
    }

}
