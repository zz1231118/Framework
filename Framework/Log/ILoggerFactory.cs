namespace Framework.Log
{
    public interface ILoggerFactory
    {
        MessageLogger CreateLogger(Level level);
    }
}
