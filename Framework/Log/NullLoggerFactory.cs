namespace Framework.Log
{
    public class NullLoggerFactory : ILoggerFactory
    {
        public static NullLoggerFactory Instance { get; } = new NullLoggerFactory();

        public void AddProvider(ILoggerProvider provider)
        { }

        public ILogger CreateLogger(string name)
        {
            return NullLogger.Instance;
        }

        sealed class NullLogger : ILogger
        {
            public static NullLogger Instance { get; } = new NullLogger();

            public string Name => null;

            public bool IsEnabled(LogLevel level)
            {
                return false;
            }

            public void Flush()
            { }

            public void Log(LogLevel level, string format, params object[] args)
            { }

            public void Dispose()
            { }
        }
    }
}
