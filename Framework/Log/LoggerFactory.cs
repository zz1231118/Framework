using System;
using System.Collections.Generic;

namespace Framework.Log
{
    public class LoggerFactory : ILoggerFactory
    {
        private readonly List<ILoggerProvider> providers = new List<ILoggerProvider>();

        public void AddProvider(ILoggerProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            providers.Add(provider);
        }

        public ILogger CreateLogger(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var loggers = new ILogger[providers.Count];
            for (int i = 0; i < providers.Count; i++)
            {
                loggers[i] = providers[i].CreateLogger(name);
            }
            return new Logger(name, loggers);
        }

        sealed class Logger : ILogger
        {
            private readonly string name;
            private readonly ILogger[] loggers;
            private bool isDisposed;

            public Logger(string name, ILogger[] loggers)
            {
                this.name = name;
                this.loggers = loggers;
            }

            public string Name => name;

            private void CheckDisposed()
            {
                if (isDisposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
            }

            private void Dispose(bool disposing)
            {
                if (!isDisposed)
                {
                    try
                    {
                        foreach (var logger in loggers)
                        {
                            logger.Dispose();
                        }
                    }
                    finally
                    {
                        isDisposed = true;
                    }
                }
            }

            public bool IsEnabled(LogLevel level)
            {
                foreach (var logger in loggers)
                {
                    if (logger.IsEnabled(level))
                    {
                        return true;
                    }
                }

                return false;
            }

            public void Flush()
            {
                CheckDisposed();
                foreach (var logger in loggers)
                {
                    logger.Flush();
                }
            }

            public void Log(LogLevel level, string format, params object[] args)
            {
                CheckDisposed();
                foreach (var logger in loggers)
                {
                    if (logger.IsEnabled(level))
                    {
                        logger.Log(level, format, args);
                    }
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
