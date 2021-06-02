using System.Collections.Generic;

namespace Framework.Data.Entry
{
    public interface IDbSet : ISet
    {
        RowEntry Add(object entity);

        RowEntry Modify(object entity);

        RowEntry Remove(object entity);

        void AddRange(IEnumerable<object> entities);

        void ModifyRange(IEnumerable<object> entities);

        void RemoveRange(IEnumerable<object> entities);
    }

    public interface IDbSet<in T> : ISet
    {
        RowEntry Add(T entity);

        RowEntry Modify(T entity);

        RowEntry Remove(T entity);

        void AddRange(IEnumerable<T> entities);

        void ModifyRange(IEnumerable<T> entities);

        void RemoveRange(IEnumerable<T> entities);
    }
}
