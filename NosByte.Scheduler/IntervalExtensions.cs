using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core.Scheduler;

namespace NosByte.Scheduler
{
    public static class IntervalExtensions
    {
        public static string ToHangFireFormat(this Interval interval)
        {
            return $"{(interval.Minute > 0 ? $"{interval.Minute}" : "*")} {(interval.Hour > 0 ? $"{interval.Hour}" : "*")} * * *";
        }
    }

}
