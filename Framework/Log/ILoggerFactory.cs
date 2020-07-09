namespace Framework.Log
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(string name);

        void AddProvider(ILoggerProvider provider);
    }
}
