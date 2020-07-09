using System;

namespace Framework.Log
{
    public class ConsoleLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string name)
        {
            return new ConsoleLogger(name);
        }

        class ConsoleLogger : BaseLogger
        {
            public ConsoleLogger(string name)
                : base(name)
            { }

            protected override void WriteMessage(string message)
            {
                Console.WriteLine(message);
            }
        }
    }
}
