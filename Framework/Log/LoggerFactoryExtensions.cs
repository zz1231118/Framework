using System;

namespace Framework.Log
{
    public static class LoggerFactoryExtensions
    {
        public static void AddProvider<T>(this ILoggerFactory loggerFactory)
            where T : ILoggerProvider, new()
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            var provider = Activator.CreateInstance<T>();
            loggerFactory.AddProvider(provider);
        }
    }
}
