namespace Framework.Statistics
{
    public interface ICounter
    {
        string Name { get; }
    }

    public interface ICounter<out T> : ICounter
        where T : struct
    {
        T Value { get; }

        T Reset();
    }
}
