using System;

namespace Framework.Log
{
    public class ConsoleLoggerFactory : ILoggerFactory
    {
        public MessageLogger CreateLogger(Level level)
        {
            return new ConsoleLogger(level);
        }

        class ConsoleLogger : MessageLogger
        {
            public ConsoleLogger(Level level)
                : base(level)
            { }

            public override void Log(string message)
            {
                Console.WriteLine(message);
            }
        }
    }
}
