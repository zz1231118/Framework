using System.Collections.Generic;

namespace Framework.Data.Entry
{
    public interface IDbSet : ISet
    {
        void Add(object entity);

        void Modify(object entity);

        void Remove(object entity);

        void AddRange(IEnumerable<object> entities);

        void ModifyRange(IEnumerable<object> entities);

        void RemoveRange(IEnumerable<object> entities);
    }

    public interface IDbSet<in T> : ISet
    {
        void Add(T entity);

        void Modify(T entity);

        void Remove(T entity);

        void AddRange(IEnumerable<T> entities);

        void ModifyRange(IEnumerable<T> entities);

        void RemoveRange(IEnumerable<T> entities);
    }
}
