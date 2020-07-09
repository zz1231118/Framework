using System;

namespace Framework.Log
{
    public abstract class BaseLogger : ILogger
    {
        private volatile bool isDisposed;
        private readonly string name;

        protected BaseLogger(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            this.name = name;
        }

        protected bool IsDisposed => isDisposed;

        public string Name => name;

        protected abstract void WriteMessage(string message);

        protected virtual string MessageFormat(LogLevel level, string message)
        {
            return string.Format("[{0}] [{1}] {2}", DateTime.Now.ToString("HH:mm:ss.fff"), level.ToString(), message);
        }

        protected void CheckDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;
            }
        }

        public bool IsEnabled(LogLevel level)
        {
            return level >= Logger.Level;
        }

        public virtual void Flush()
        { }

        public void Log(LogLevel level, string format, params object[] args)
        {
            CheckDisposed();
            if (format == null)
                throw new ArgumentNullException(nameof(format));
            if (!IsEnabled(level))
                return;

            string message;
            if (args.Length > 0)
            {
                try
                {
                    message = string.Format(format, args);
                }
                catch (Exception ex)
                {
                    Logger.NotifyUnhandledExceptionEvent(new LoggerUnhandledExceptionEventArgs(this, ex));
                    return;
                }
            }
            else
            {
                message = format;
            }
            try
            {
                message = MessageFormat(level, message);
            }
            catch (Exception ex)
            {
                Logger.NotifyUnhandledExceptionEvent(new LoggerUnhandledExceptionEventArgs(this, ex));
                return;
            }

            WriteMessage(message);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
