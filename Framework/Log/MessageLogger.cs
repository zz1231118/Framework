using System;

namespace Framework.Log
{
    public abstract class MessageLogger : IDisposable
    {
        private readonly Level level;
        private bool isDisposed;

        public MessageLogger(Level level)
        {
            this.level = level;
        }

        protected bool IsDisposed => isDisposed;

        public Level Level => level;

        public abstract void Log(string message);

        public virtual void Flush()
        { }

        protected virtual void CheckDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        { }

        public void Dispose()
        {
            if (!isDisposed)
            {
                try
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }
                finally
                {
                    isDisposed = true;
                }
            }
        }
    }
}
