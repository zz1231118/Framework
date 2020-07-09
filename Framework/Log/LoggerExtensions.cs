namespace Framework.Log
{
    public static class LoggerExtensions
    {
        public static void Trace(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Trace, format, args);
        }

        public static void Debug(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Debug, format, args);
        }

        public static void Info(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Info, format, args);
        }

        public static void Warn(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Warn, format, args);
        }

        public static void Error(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Error, format, args);
        }

        public static void Fatal(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Fatal, format, args);
        }
    }
}