using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Data.Entry
{
    internal class DbSet<T> : IDbSet<T>, IDbSet, IInternalSet
        where T : class
    {
        private readonly DbContext dbContext;
        private readonly Type entityType;
        private readonly IEntitySchema entitySchema;
        private readonly Dictionary<T, RowEntry> entityStore = new Dictionary<T, RowEntry>();

        internal DbSet(DbContext dbContext, Type entityType, IEntitySchema entitySchema)
        {
            this.dbContext = dbContext;
            this.entityType = entityType;
            this.entitySchema = entitySchema;
        }

        public DbContext DbContext => dbContext;

        public Type EntityType => entityType;

        public IEntitySchema EntitySchema => entitySchema;

        public int Count => entityStore.Count;

        private RowEntry GetRowEntry(T entity)
        {
            if (!entityStore.TryGetValue(entity, out RowEntry entry))
            {
                entry = new RowEntry(entity, EntityState.Unchanged);
                entityStore[entity] = entry;
            }

            return entry;
        }

        public RowEntry[] GetRowEntries()
        {
            return entityStore.Values.ToArray();
        }

        public void Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entry = GetRowEntry(entity);
            entry.State = EntityState.Added;
        }

        public void Modify(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entry = GetRowEntry(entity);
            entry.State = EntityState.Modified;
        }

        public void Remove(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entry = GetRowEntry(entity);
            entry.State = EntityState.Deleted;
        }

        public void AddRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            RowEntry entry;
            foreach (var entity in entities)
            {
                entry = GetRowEntry(entity);
                entry.State = EntityState.Added;
            }
        }

        public void ModifyRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            RowEntry entry;
            foreach (var entity in entities)
            {
                entry = GetRowEntry(entity);
                entry.State = EntityState.Modified;
            }
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            RowEntry entry;
            foreach (var entity in entities)
            {
                entry = GetRowEntry(entity);
                entry.State = EntityState.Deleted;
            }
        }

        public void Clear()
        {
            foreach (var entry in entityStore.Values)
            {
                entry.State = EntityState.Detached;
            }

            entityStore.Clear();
        }

        void IDbSet.Add(object entity)
        {
            Add((T)entity);
        }

        void IDbSet.Modify(object entity)
        {
            Modify((T)entity);
        }

        void IDbSet.Remove(object entity)
        {
            Remove((T)entity);
        }

        void IDbSet.AddRange(IEnumerable<object> entities)
        {
            AddRange((IEnumerable<T>)entities);
        }

        void IDbSet.ModifyRange(IEnumerable<object> entities)
        {
            ModifyRange((IEnumerable<T>)entities);
        }

        void IDbSet.RemoveRange(IEnumerable<object> entities)
        {
            RemoveRange((IEnumerable<T>)entities);
        }
    }
}
