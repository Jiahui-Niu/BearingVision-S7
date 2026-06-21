using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    public class ThreadPoolEx : IDisposable
    {
        private int _maxThreadCnt = 2;
        private int _currentThreadCnt = 0;
        private AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        private bool isDisposed = false;

        public ThreadPoolEx(int maxThreadCnt)
        {
            if (maxThreadCnt <= 0)
                throw new ArgumentException("maxThreadCnt must be greater than 0");

            _maxThreadCnt = maxThreadCnt;
        }

        public void QueueUserWorkItem(WaitCallback callback, object state)
        {
            if (Volatile.Read(ref _currentThreadCnt) >= _maxThreadCnt)
            {
                autoResetEvent.WaitOne();
            }

            if (isDisposed)
            {
                return;
            }

            ThreadPool.QueueUserWorkItem((o) => {
                Interlocked.Increment(ref _currentThreadCnt);
                callback.Invoke(state);
                Interlocked.Decrement(ref _currentThreadCnt);

                if (Volatile.Read(ref _currentThreadCnt) < _maxThreadCnt)
                {
                    autoResetEvent.Set();
                }

            }, null);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                autoResetEvent.Set();
            }
        }
    }
}
