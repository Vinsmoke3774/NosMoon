using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using OpenNos.Core.Scheduler;

namespace NosByte.Scheduler
{
    public class BackgroundJobScheduler : IBackgroundJobScheduler
    {
        public BackgroundJobScheduler()
        {
            if (SchedulerBootstrap.ServerStarted)
            {
                return;
            }

            SchedulerBootstrap.StartScheduler();
        }

        public void Add<T>(Expression<Action<T>> method)
        {
            BackgroundJob.Enqueue(method);
        }

        public void Add(Expression<Action> method)
        {
            BackgroundJob.Enqueue(method);
        }

        public void Add<T>(Expression<Func<T, Task>> method)
        {
            BackgroundJob.Enqueue(method);
        }

        public void Add<T>(Expression<Func<Task>> method)
        {
            BackgroundJob.Enqueue(method);
        }

        public void ContinueWith<T>(string parentId, Expression<Action<T>> methodCall)
        {
            BackgroundJob.ContinueJobWith(parentId, methodCall);
        }

        public void ContinueWith<T>(string parentId, Expression<Action<T>> methodCall, JobContinuationOptions options)
        {
            BackgroundJob.ContinueJobWith(parentId, methodCall);
        }

        public void ContinueWith(string parentId, Expression<Action> methodCall)
        {
            BackgroundJob.ContinueJobWith(parentId, methodCall);
        }

        public void ContinueWith(string parentId, Expression<Action> methodCall, JobContinuationOptions options)
        {
            BackgroundJob.ContinueJobWith(parentId, methodCall);
        }

        public void ContinueWith<T>(string parentId, Expression<Func<T, Task>> methodCall)
        {
            BackgroundJob.ContinueJobWith(parentId, methodCall);
        }

        public void Requeue(string jobId)
        {
            BackgroundJob.Requeue(jobId);
        }

        public void Requeue(string jobId, string fromState)
        {
            BackgroundJob.Requeue(jobId, fromState);
        }

        public void ScheduleJob<T>(Expression<Action<T>> methodCall, DateTimeOffset enqueueAt)
        {
            BackgroundJob.Schedule(methodCall, enqueueAt);
        }

        public void ScheduleJob<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        {
            BackgroundJob.Schedule(methodCall, delay);
        }

        public void ScheduleJob(Expression<Action> methodCall, TimeSpan delay)
        {
            BackgroundJob.Schedule(methodCall, delay);
        }

        public void ScheduleJob<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
        {
            BackgroundJob.Schedule(methodCall, enqueueAt);
        }

        public void Delete(string jobId)
        {
            BackgroundJob.Delete(jobId);
        }

        public void Delete(string jobId, string fromState)
        {
            BackgroundJob.Delete(jobId, fromState);
        }
    }

}
