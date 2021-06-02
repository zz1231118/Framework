namespace Framework.Log
{
    public class NullLoggerFactory : ILoggerFactory
    {
        public static NullLoggerFactory Instance { get; } = new NullLoggerFactory();

        public MessageLogger CreateLogger(Level level)
        {
            return new NullLogger(level);
        }

        private sealed class NullLogger : MessageLogger
        {
            public NullLogger(Level level)
                : base(level)
            { }

            public override void Log(string message)
            { }
        }
    }
}
