using System;
using System.Threading;

namespace Framework.Net.Remoting
{
    public class SyncTimer : BaseDisposed
    {
        private readonly TimerCallback _callback;
        private Timer _timer;
        private int isInTimer;
        private Thread? _executeThread;
        private Timer _executeTimer;

        public int DueTime { get; set; }
        public int Period { get; set; }
        public int ExecuteTimeout { get; set; }

        public SyncTimer(TimerCallback callback, int dueTime, int period, int executeTimeout = 60000)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            _callback = callback;
            DueTime = dueTime;
            Period = period;
            ExecuteTimeout = executeTimeout;
        }
        public SyncTimer(TimerCallback callback, TimeSpan dueTime, TimeSpan period, TimeSpan executeTimeout)
            : this(callback, (int)dueTime.TotalMilliseconds, (int)period.TotalMilliseconds, (int)executeTimeout.TotalMilliseconds)
        { }

        public virtual void Start()
        {
            _timer = new Timer(InternalDoWork, null, DueTime, Period);
            _executeTimer = new Timer(AbortExecute, null, Timeout.Infinite, Timeout.Infinite);
        }
        public virtual void Stop()
        {
            _timer.Dispose();
            while (Interlocked.CompareExchange(ref isInTimer, 1, 0) == 1)
                Thread.Sleep(10);
        }
        private void AbortExecute(object state)
        {
            try
            {
                if (isInTimer == 1)
                {
                    var executeThread = Interlocked.Exchange(ref _executeThread, null);
                    if (executeThread != null)
                    {
                        executeThread.Abort();
                    }
                    isInTimer = 0;
                }
            }
            catch (Exception)
            { }
        }
        private void InternalDoWork(object state)
        {
            if (Interlocked.CompareExchange(ref isInTimer, 1, 0) == 1)
                return;

            try
            {
                _executeThread = Thread.CurrentThread;
                _executeTimer.Change(ExecuteTimeout, Timeout.Infinite);
                _callback(state);
            }
            catch (Exception)
            { }
            finally
            {
                _executeThread = null;
                _executeTimer.Change(Timeout.Infinite, Timeout.Infinite);
                Interlocked.Exchange(ref isInTimer, 0);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                try
                {
                    _timer?.Dispose();
                    _executeTimer?.Dispose();
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }
    }
}