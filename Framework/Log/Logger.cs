using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Framework.Log
{
    public static class Logger
    {
        private static readonly object root = new object();
        private static readonly ConcurrentDictionary<string, ILogger> loggers = new ConcurrentDictionary<string, ILogger>();
        private static LoggerSetting setting;
        private static LogLevel level = LogLevel.Warn;
        private static ILoggerFactory loggerFactory = NullLoggerFactory.Instance;
        private static Timer checkFlushTimer;

        static Logger()
        {
            Initialize(LoggerSetting.Default);
        }

        public static LoggerSetting Setting
        {
            get => setting;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                Initialize(value);
            }
        }

        public static LogLevel Level
        {
            get => level;
            set => level = value;
        }

        public static ILoggerFactory LoggerFactory
        {
            get => loggerFactory;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                List<ILogger> oldLoggers;
                lock (root)
                {
                    oldLoggers = new List<ILogger>(loggers.Values);
                    loggerFactory = value;
                    loggers.Clear();
                }

                foreach (var logger in oldLoggers)
                {
                    logger.Flush();
                    logger.Dispose();
                }
            }
        }

        public static event EventHandler<LoggerEventArgs> NewLogger;

        public static event EventHandler<LoggerUnhandledExceptionEventArgs> UnhandledException;

        private static void Initialize(LoggerSetting setting)
        {
            lock (root)
            {
                if (checkFlushTimer != null)
                {
                    checkFlushTimer.Dispose();
                    checkFlushTimer = null;
                }
                if (setting.IsAutoFlush)
                {
                    checkFlushTimer = new Timer(new TimerCallback(CheckFlushTimer_Callbacked), null, setting.FlushInterval, setting.FlushInterval);
                }

                Logger.setting = setting;
            }
        }

        private static void CheckFlushTimer_Callbacked(object obj)
        {
            Flush();
        }

        private static ILogger CreateLogger(string name)
        {
            var logger = loggerFactory.CreateLogger(name);
            var newLogger = NewLogger;
            if (newLogger != null)
            {
                try
                {
                    newLogger(null, new LoggerEventArgs(logger));
                }
                catch (Exception ex)
                {
                    NotifyUnhandledExceptionEvent(new LoggerUnhandledExceptionEventArgs(logger, ex));
                }
            }
            return logger;
        }

        public static bool IsEnabled(LogLevel level)
        {
            return level >= Logger.level;
        }

        public static ILogger GetLogger(string name)
        {
            lock (root)
            {
                if (!loggers.TryGetValue(name, out ILogger logger))
                {
                    logger = loggerFactory.CreateLogger(name);
                    loggers[name] = logger;
                }

                return logger;
            }
        }

        public static ILogger GetLogger(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return GetLogger(type.FullName);
        }

        public static ILogger GetLogger<T>()
        {
            return GetLogger(typeof(T));
        }

        public static void Flush()
        {
            foreach (var logger in loggers.Values)
            {
                logger.Flush();
            }
        }

        public static void Shutdown()
        {
            var flushTimer = checkFlushTimer;
            if (flushTimer != null)
            {
                checkFlushTimer = null;
                flushTimer.Dispose();
            }

            foreach (var logger in loggers.Values)
            {
                logger.Dispose();
            }

            loggers.Clear();
        }

        public static void NotifyUnhandledExceptionEvent(LoggerUnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(null, e);
            if (!e.Observed)
            {
                throw e.Exception;
            }
        }
    }
}