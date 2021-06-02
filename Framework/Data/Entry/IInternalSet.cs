namespace Framework.Data.Entry
{
    internal interface IInternalSet : ISet
    {
        IEntitySchema EntitySchema { get; }
    }
}
