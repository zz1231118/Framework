using System;

namespace Framework.Log
{
    public class LoggerEventArgs : EventArgs
    {
        internal LoggerEventArgs(ILogger logger)
        {
            Logger = logger;
        }

        internal LoggerEventArgs(ILogger logger, LogLevel level, string message)
        {
            Logger = logger;
            Level = level;
            Message = message;
        }

        public ILogger Logger { get; }

        public LogLevel Level { get; }

        public string Message { get; }
    }
}