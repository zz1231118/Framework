using System;
using System.Collections.Generic;

namespace Framework.Data.Entry
{
    public interface ISet
    {
        int Count { get; }

        Type EntityType { get; }

        IReadOnlyCollection<RowEntry> RowEntries { get; }

        void Clear();
    }
}
