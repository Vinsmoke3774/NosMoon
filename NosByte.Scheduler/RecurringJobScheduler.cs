using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using OpenNos.Core.Scheduler;
using OpenNos.Domain;

namespace NosByte.Scheduler
{
    public class RecurringJobScheduler : IRecurringJobScheduler
    {
        public RecurringJobScheduler()
        {
            if (SchedulerBootstrap.ServerStarted)
            {
                return;
            }

            SchedulerBootstrap.StartScheduler();
        }

        public void Delete(string jobId)
        {
            RecurringJob.RemoveIfExists(jobId);
        }

        public void Delete(string jobId, string fromState)
        {
            RecurringJob.RemoveIfExists(jobId);
        }

        public void Add<T>(Expression<Action<T>> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(methodCall, cronExpression, timeZone, queue);
        }

        public void Add<T>(Expression<Action<T>> methodCall, Interval interval, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(methodCall, interval.ToHangFireFormat, timeZone, queue);
        }

        public void Add<T>(Expression<Action<T>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(methodCall, cronExpression, timeZone, queue);
        }

        public void Add(Expression<Action> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(methodCall, cronExpression, timeZone, queue);
        }

        public void Add(Expression<Action> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(methodCall, cronExpression, timeZone, queue);
        }

        public void Add(Expression<Action> methodCall, Interval interval, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(methodCall, interval.ToHangFireFormat, timeZone, queue);
        }

        public void Add<T>(Expression<Func<T, Task>> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(methodCall, cronExpression, timeZone, queue);
        }

        public void Add<T>(Expression<Func<T, Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(methodCall, cronExpression, timeZone, queue);
        }

        public void Add<T>(Expression<Func<T, Task>> methodCall, Interval interval, TimeZoneInfo timeZone = null, string queue = default)
        {
            RecurringJob.AddOrUpdate(methodCall, interval.ToHangFireFormat, timeZone, queue);
        }

        public void Add(Expression<Func<Task>> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(methodCall, cronExpression, timeZone, queue);
        }

        public void Add(Expression<Func<Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(methodCall, cronExpression, timeZone, queue);
        }

        public void Add(Expression<Func<Task>> methodCall, Interval interval, TimeZoneInfo timeZone = null, string queue = default)
        {
            RecurringJob.AddOrUpdate(methodCall, interval.ToHangFireFormat, timeZone, queue);
        }

        public void Add<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression, timeZone, queue);
        }

        public void Add<T>(string recurringJobId, Expression<Action<T>> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression, timeZone, queue);
        }

        public void Add<T>(string recurringJobId, Expression<Action<T>> methodCall, Interval interval, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(recurringJobId, methodCall, interval.ToHangFireFormat, timeZone, queue);
        }

        public void Add(string recurringJobId, Expression<Action> methodCall, string cronExpression, TimeZoneInfo timeZone = null,
            string queue = "default")
        {
            RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression, timeZone, queue);
        }

        public void Add(string recurringJobId, Expression<Action> methodCall, CronEventType type, int time, TimeZoneInfo timeZone = null,
            string queue = "default")
        {
            RecurringJob.AddOrUpdate(recurringJobId, methodCall, SchedulerUtils.GetCronTypes(type, time), timeZone, queue);
        }

        public void Add(string recurringJobId, Expression<Action> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression, timeZone, queue);
        }

        public void Add(string recurringJobId, Expression<Action> methodCall, Interval interval, TimeZoneInfo timeZone = null,
            string queue = "default")
        {
            RecurringJob.AddOrUpdate(recurringJobId, methodCall, interval.ToHangFireFormat, timeZone, queue);
        }

        public void Add<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression, timeZone, queue);
        }

        public void Add<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression, timeZone, queue);
        }

        public void Add<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, Interval interval, TimeZoneInfo timeZone = null,
            string queue = "default")
        {
            RecurringJob.AddOrUpdate(recurringJobId, methodCall, interval.ToHangFireFormat, timeZone, queue);
        }

        public void Add(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression, timeZone, queue);
        }

        public void Add(string recurringJobId, Expression<Func<Task>> methodCall, Func<string> cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression, timeZone, queue);
        }

        public void Add(string recurringJobId, Expression<Func<Task>> methodCall, Interval interval, TimeZoneInfo timeZone = null,
            string queue = "default")
        {
            RecurringJob.AddOrUpdate(recurringJobId, methodCall, interval.ToHangFireFormat, timeZone, queue);
        }
    }

}
