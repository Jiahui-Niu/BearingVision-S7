using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    /// <summary>
    /// 异步方法同步执行, 防止.Result死锁
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory _taskFactory = new
        TaskFactory(CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskContinuationOptions.None,
                    TaskScheduler.Default);

        /// <summary>
        /// 异步方法同步执行, 防止.Result死锁
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        /// <example>
        /// <code>
        ///     AsyncHelper.RunSync(() => goodsCtl.AdmittanceAsync(request))
        /// </code>
        /// </example>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        { 
            return _taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// 异步方法同步执行, 防止.Result死锁
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <example>
        /// <code>
        ///     AsyncHelper.RunSync(() => goodsCtl.AdmittanceAsync(request))
        /// </code>
        /// </example>
        public static void RunSync(Func<Task> func)
        {
            _taskFactory
                   .StartNew(func)
                   .Unwrap()
                   .GetAwaiter()
                   .GetResult();
        }

        /// <summary>
        /// 异步方法同步执行, 防止.Result死锁
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <param name="timeout">超时</param>
        /// <example>
        /// <code>
        ///     AsyncHelper.RunSync((object state) => goodsCtl.AdmittanceAsync(request, (CancellationToken)state)
        /// </code>
        /// </example>
        /// <remarks>异步中不能使用 Thread.Sleep() 等无法被取消的任务,否者超时无效 , 所有异步方法都需要使用带有取消命令的实现,否者超时无效, 超时会引发超时异常</remarks>
        /// <exception cref="System.Threading.Tasks.TaskCanceledException">超时,任务取消异常</exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static TResult RunSync<TResult>(Func<object, Task<TResult>> func, int timeout)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            var task = _taskFactory
                .StartNew(func, (object)source.Token)
                .Unwrap()
                .GetAwaiter();

            Task<TResult> resultTask = Task.Run(() => { return task.GetResult(); });
            int index = Task.WaitAny(resultTask, Task.Delay(timeout));

            if (index == 1)
            {
                source.Cancel();
            }

            //wait result anyway, prevent task doesn't listen to cancel event, eg. Thread.Sleep();
            return resultTask.Result;
        }

        /// <summary>
        /// 异步方法同步执行, 防止.Result死锁
        /// </summary>
        /// <param name="func"></param>
        /// <param name="timeout">超时</param>
        /// <example>
        /// <code>
        ///     AsyncHelper.RunSync((object state) => goodsCtl.AdmittanceAsync(request, (CancellationToken)state), 2000)
        /// </code>
        /// </example>
        /// <remarks>异步中不能使用 Thread.Sleep() 等无法被取消的任务,否者超时无效 ,所有异步方法都需要使用带有取消命令的实现,否者超时无效, 超时会引发超时异常</remarks>
        /// <exception cref="System.Threading.Tasks.TaskCanceledException">超时,任务取消异常</exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void RunSync(Func<object, Task> func, int timeout)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            var task = _taskFactory.StartNew(func, (object)source.Token)
                .Unwrap()
                .GetAwaiter();

            Task resultTask = Task.Run(() => { task.GetResult(); });
            int index = Task.WaitAny(resultTask, Task.Delay(timeout));

            if (index == 1)
            {
                source.Cancel();
            }

            //wait result anyway, prevent task doesn't listen to cancel event, eg. Thread.Sleep();
            resultTask.Wait();
        }
    }
}
