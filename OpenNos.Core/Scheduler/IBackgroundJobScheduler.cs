using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Hangfire;

namespace OpenNos.Core.Scheduler
{
    public interface IBackgroundJobScheduler : IJobScheduler
    {
        /// <summary>
        /// Enqueue a new job to the BackgroundJob queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        void Add<T>(Expression<Action<T>> method);

        /// <summary>
        /// Enqueue a new job to the BackgroundJob queue
        /// </summary>
        /// <param name="method"></param>
        void Add(Expression<Action> method);

        /// <summary>
        /// Enqueue a new job to the BackgroundJob queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        void Add<T>(Expression<Func<T, Task>> method);

        /// <summary>
        /// Enqueue a new job to the BackgroundJob queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        void Add<T>(Expression<Func<Task>> method);

        /// <summary>
        /// Creates a new background job started after the parent is executed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parentId"></param>
        /// <param name="methodCall"></param>
        void ContinueWith<T>(string parentId, Expression<Action<T>> methodCall);

        /// <summary>
        /// Creates a new background job started after the parent is executed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parentId"></param>
        /// <param name="methodCall"></param>
        /// <param name="options"></param>
        void ContinueWith<T>(string parentId, Expression<Action<T>> methodCall, JobContinuationOptions options);

        /// <summary>
        /// Creates a new background job started after the parent is executed
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="methodCall"></param>
        void ContinueWith(string parentId, Expression<Action> methodCall);

        /// <summary>
        /// Creates a new background job started after the parent is executed
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="methodCall"></param>
        /// <param name="options"></param>
        void ContinueWith(string parentId, Expression<Action> methodCall, JobContinuationOptions options);

        /// <summary>
        /// Creates a new background job started after the parent is executed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parentId"></param>
        /// <param name="methodCall"></param>
        void ContinueWith<T>(string parentId, Expression<Func<T, Task>> methodCall);

        /// <summary>
        /// Changes the state of the specified jobId
        /// </summary>
        /// <param name="jobId"></param>
        void Requeue(string jobId);

        /// <summary>
        /// Changes the state of the specified jobid. If fromState is not null
        /// State change will be performed only if the current state name is equal to the given value
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="fromState"></param>
        void Requeue(string jobId, string fromState);

        /// <summary>
        /// Creates a new BackgroundJob
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        /// <param name="enqueueAt"></param>
        void ScheduleJob<T>(Expression<Action<T>> methodCall, DateTimeOffset enqueueAt);

        /// <summary>
        /// Creates a new background job
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        /// <param name="delay"></param>
        void ScheduleJob<T>(Expression<Action<T>> methodCall, TimeSpan delay);

        /// <summary>
        /// Creates a new background job
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        /// <param name="delay"></param>
        void ScheduleJob(Expression<Action> methodCall, TimeSpan delay);

        /// <summary>
        /// Creates a new background job
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        /// <param name="enqueueAt"></param>
        void ScheduleJob<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);
    }

}
