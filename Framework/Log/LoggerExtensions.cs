namespace Framework.Log
{
    public static class LoggerExtensions
    {
        public static void Trace(this ILogger logger, string format, params object[] args)
        {
            logger.Log(Level.Trace, format, args);
        }

        public static void Debug(this ILogger logger, string format, params object[] args)
        {
            logger.Log(Level.Debug, format, args);
        }

        public static void Info(this ILogger logger, string format, params object[] args)
        {
            logger.Log(Level.Info, format, args);
        }

        public static void Warn(this ILogger logger, string format, params object[] args)
        {
            logger.Log(Level.Warn, format, args);
        }

        public static void Error(this ILogger logger, string format, params object[] args)
        {
            logger.Log(Level.Error, format, args);
        }

        public static void Fatal(this ILogger logger, string format, params object[] args)
        {
            logger.Log(Level.Fatal, format, args);
        }
    }
}