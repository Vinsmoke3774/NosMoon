using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Domain;

namespace OpenNos.Core.Scheduler
{
    public interface IRecurringJobScheduler : IJobScheduler
    {
        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add<T>(Expression<Action<T>> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new recurring job based on an Interval converted to a CRON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        /// <param name="interval"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add<T>(Expression<Action<T>> methodCall, Interval interval, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add<T>(Expression<Action<T>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add(Expression<Action> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add(Expression<Action> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new recurring job based on an Interval converted to a CRON
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="interval"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add(Expression<Action> methodCall, Interval interval, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add<T>(Expression<Func<T, Task>> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add<T>(Expression<Func<T, Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new recurring job based on an Interval converted to a CRON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        /// <param name="interval"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add<T>(Expression<Func<T, Task>> methodCall, Interval interval, TimeZoneInfo timeZone = null, string queue = default);

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add(Expression<Func<Task>> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add(Expression<Func<Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new recurring job based on an Interval converted to a CRON
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="interval"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add(Expression<Func<Task>> methodCall, Interval interval, TimeZoneInfo timeZone = null, string queue = default);

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recurringJobId"></param>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recurringJobId"></param>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add<T>(string recurringJobId, Expression<Action<T>> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new recurring job based on an Interval converted to a CRON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recurringJobId"></param>
        /// <param name="methodCall"></param>
        /// <param name="interval"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add<T>(string recurringJobId, Expression<Action<T>> methodCall, Interval interval, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <param name="recurringJobId"></param>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add(string recurringJobId, Expression<Action> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recurringJobId"></param>
        /// <param name="methodCall"></param>
        /// <param name="type"></param>
        /// <param name="time"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add(string recurringJobId, Expression<Action> methodCall, CronEventType type, int time, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <param name="recurringJobId"></param>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add(string recurringJobId, Expression<Action> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new recurring job based on an Interval converted to a CRON
        /// </summary>
        /// <param name="recurringJobId"></param>
        /// <param name="methodCall"></param>
        /// <param name="interval"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add(string recurringJobId, Expression<Action> methodCall, Interval interval, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recurringJobId"></param>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recurringJobId"></param>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new recurring job based on an Interval converted to a CRON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recurringJobId"></param>
        /// <param name="methodCall"></param>
        /// <param name="interval"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, Interval interval, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <param name="recurringJobId"></param>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new Recurring job
        /// </summary>
        /// <param name="recurringJobId"></param>
        /// <param name="methodCall"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add(string recurringJobId, Expression<Func<Task>> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default");

        /// <summary>
        /// Adds a new recurring job based on an Interval converted to a CRON
        /// </summary>
        /// <param name="recurringJobId"></param>
        /// <param name="methodCall"></param>
        /// <param name="interval"></param>
        /// <param name="timeZone"></param>
        /// <param name="queue"></param>
        void Add(string recurringJobId, Expression<Func<Task>> methodCall, Interval interval, TimeZoneInfo timeZone = null, string queue = "default");
    }

}
