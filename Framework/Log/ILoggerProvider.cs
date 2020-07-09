namespace Framework.Log
{
    public interface ILoggerProvider
    {
        ILogger CreateLogger(string name);
    }
}