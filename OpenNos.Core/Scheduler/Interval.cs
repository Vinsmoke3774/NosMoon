using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core.Scheduler
{
    public class Interval
    {
        public Interval EveryMinute(byte minute)
        {
            if (minute > 59)
            {
                throw new InvalidOperationException("Minute should be between 0~59.");
            }

            Minute = minute;
            return this;
        }

        public Interval EveryHour(byte hour)
        {
            if (hour > 23)
            {
                throw new InvalidOperationException("Hour should be between 0~23.");
            }

            Hour = hour;
            return this;
        }

        public Interval EveryHourAndMinutes(byte hour, byte minute)
        {
            if (hour > 23)
            {
                throw new InvalidOperationException("Hour should be between 0~23.");
            }

            Hour = hour;

            if (minute > 59)
            {
                throw new InvalidOperationException("Minute should be between 0~59.");
            }

            Minute = minute;
            return this;
        }

        public byte Minute { get; private set; } = 0;

        public byte Hour { get; private set; } = 0;
    }

}
