using System;
using System.Threading;

namespace Framework
{
    public abstract class BaseDisposed : IDisposable
    {
        private bool _isDisposed;

        protected bool IsDisposed => _isDisposed;

        protected void CheckDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public abstract class ThreadSafetyBaseDisposed : IDisposable
    {
        private const int DisposedFlag = 1;
        private int isDisposed;

        protected bool IsDisposed
        {
            get
            {
                var val = isDisposed;
                Thread.MemoryBarrier();
                return val == DisposedFlag;
            }
        }

        protected void CheckDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            Interlocked.CompareExchange(ref isDisposed, DisposedFlag, 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}