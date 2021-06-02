using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Framework.Log
{
    public static class Logger
    {
        private static readonly object root = new object();
        private static readonly ConcurrentBag<ILoggerFactory> factories = new ConcurrentBag<ILoggerFactory>();
        private static readonly ConcurrentDictionary<Level, MessageLogger[]> messageLoggers = new ConcurrentDictionary<Level, MessageLogger[]>();
        private static readonly ConcurrentDictionary<string, ILogger> loggers = new ConcurrentDictionary<string, ILogger>();
        private static readonly Func<string, ILogger> valueFactory = key => new ScopeLogger(key);
        private static ILoggerConfiguration configuration;
        private static Timer? autoFlushTimer;

        static Logger()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(DomainUnload);
            Initialize(LoggerConfiguration.Default);
        }

        /// <summary>
        /// Log 输出等级。默认：<see cref="Level.Info" />。
        /// </summary>
        public static Level Level { get; set; } = Level.Info;

        public static ILoggerConfiguration Configuration
        {
            get => configuration;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                Initialize(value);
            }
        }

        public static IReadOnlyCollection<ILoggerFactory> Factories => factories;

        public static IReadOnlyCollection<ILogger> Loggers
        {
            get
            {
                var collection = loggers.Values;
                return collection as IReadOnlyCollection<ILogger> ?? new List<ILogger>(collection);
            }
        }

        public static event EventHandler<LoggerUnhandledExceptionEventArgs>? UnhandledException;

        private static void Initialize(ILoggerConfiguration configuration)
        {
            lock (root)
            {
                if (autoFlushTimer != null)
                {
                    autoFlushTimer.Dispose();
                    autoFlushTimer = null;
                }
                if (configuration.IsAutoFlush)
                {
                    autoFlushTimer = new Timer(new TimerCallback(AutoFlushTimerCallbacked), null, configuration.FlushInterval, configuration.FlushInterval);
                }

                Logger.configuration = configuration;
            }
        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            Shutdown();
        }

        private static void DomainUnload(object sender, EventArgs e)
        {
            Shutdown();
        }

        private static void AutoFlushTimerCallbacked(object obj)
        {
            try
            {
                Flush();
            }
            catch (Exception ex)
            {
                var e = new LoggerUnhandledExceptionEventArgs(ex);
                NotifyUnhandledExceptionEvent(e);
                return;
            }
        }

        private static MessageLogger[] GetLoggers(Level level)
        {
            if (!messageLoggers.TryGetValue(level, out MessageLogger[] value))
            {
                lock (root)
                {
                    if (!messageLoggers.TryGetValue(level, out value))
                    {
                        var loggers = new List<MessageLogger>();
                        foreach (var factory in factories)
                        {
                            loggers.Add(factory.CreateLogger(level));
                        }

                        value = loggers.ToArray();
                        messageLoggers[level] = value;
                    }
                }
            }
            return value;
        }

        public static void AddFactory(ILoggerFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            factories.Add(factory);
        }

        public static void AddFactory<T>(Action<T> initializer = null)
            where T : ILoggerFactory, new()
        {
            var factory = Activator.CreateInstance<T>();
            initializer?.Invoke(factory);
            factories.Add(factory);
        }

        public static bool IsEnabled(Level level)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));

            return level >= Level;
        }

        public static ILogger GetLogger(string name)
        {
            return loggers.GetOrAdd(name, valueFactory);
        }

        public static ILogger GetLogger(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return loggers.GetOrAdd(type.FullName, valueFactory);
        }

        public static ILogger GetLogger<T>()
        {
            return loggers.GetOrAdd(typeof(T).FullName, valueFactory);
        }

        public static void Flush()
        {
            foreach (var loggers in messageLoggers.Values)
            {
                foreach (var logger in loggers)
                {
                    logger.Flush();
                }
            }
        }

        public static void Shutdown()
        {
            lock (root)
            {
                if (autoFlushTimer != null)
                {
                    autoFlushTimer.Dispose();
                    autoFlushTimer = null;
                }
            }

            foreach (var loggers in messageLoggers.Values)
            {
                foreach (var logger in loggers)
                {
                    logger.Flush();
                    logger.Dispose();
                }
            }

            messageLoggers.Clear();
        }

        public static void NotifyUnhandledExceptionEvent(LoggerUnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(null, e);
            if (!e.Observed)
            {
                throw e.Exception;
            }
        }

        class ScopeLogger : ILogger
        {
            private readonly string name;

            public ScopeLogger(string name)
            {
                this.name = name;
            }

            public string Name => name;

            public void Log(Level level, string format, params object[] args)
            {
                if (Logger.IsEnabled(level))
                {
                    var message = format;
                    var loggers = Logger.GetLoggers(level);
                    if (args.Length > 0)
                    {
                        try
                        {
                            message = string.Format(message, args);
                        }
                        catch (Exception ex)
                        {
                            var e = new LoggerUnhandledExceptionEventArgs(ex);
                            Logger.NotifyUnhandledExceptionEvent(e);
                            return;
                        }
                    }
                    message = string.Format("[{0}] [{1}] {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), name, message);
                    foreach (var logger in loggers)
                    {
                        logger.Log(message);
                    }
                }
            }
        }
    }
}
