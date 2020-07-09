using System;

namespace Framework.Log
{
    public class LoggerUnhandledExceptionEventArgs : EventArgs
    {
        internal LoggerUnhandledExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }

        internal LoggerUnhandledExceptionEventArgs(ILogger logger, Exception exception)
        {
            Logger = logger;
            Exception = exception;
        }

        public ILogger Logger { get; }

        public Exception Exception { get; }

        public bool Observed { get; private set; }

        public void SetObserved()
        {
            Observed = true;
        }
    }
}