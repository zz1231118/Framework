using System;

namespace Framework.Data.Entry
{
    internal interface IInternalSet
    {
        int Count { get; }

        Type EntityType { get; }

        IEntitySchema EntitySchema { get; }

        RowEntry[] GetRowEntries();
    }
}
