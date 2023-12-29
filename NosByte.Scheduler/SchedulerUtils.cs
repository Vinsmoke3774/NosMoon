using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using OpenNos.Domain;

namespace NosByte.Scheduler
{
    public class SchedulerUtils
    {
        public static string GetCronTypes(CronEventType type, int time)
        {
            switch (type)
            {
                case CronEventType.Minute:
                    return Cron.Hourly(time);
                case CronEventType.Hour:
                    return Cron.Daily(time);
                case CronEventType.Daily:
                    return Cron.Weekly((DayOfWeek)time);
                case CronEventType.Weekly:
                    return Cron.Monthly(time);
                case CronEventType.Monthly:
                    return Cron.Yearly(time);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }

}
