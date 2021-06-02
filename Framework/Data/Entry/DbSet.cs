using System;
using System.Collections.Generic;

namespace Framework.Data.Entry
{
    internal class DbSet<T> : IDbSet<T>, IDbSet, IInternalSet
        where T : class
    {
        private readonly DbContext dbContext;
        private readonly Type entityType;
        private readonly IEntitySchema entitySchema;
        private readonly Dictionary<T, RowEntry> rowEntries = new Dictionary<T, RowEntry>();

        internal DbSet(DbContext dbContext, Type entityType, IEntitySchema entitySchema)
        {
            this.dbContext = dbContext;
            this.entityType = entityType;
            this.entitySchema = entitySchema;
        }

        public DbContext DbContext => dbContext;

        public Type EntityType => entityType;

        public IEntitySchema EntitySchema => entitySchema;

        public int Count => rowEntries.Count;

        public IReadOnlyCollection<RowEntry> RowEntries => rowEntries.Values;

        private RowEntry GetRowEntry(T entity)
        {
            if (!rowEntries.TryGetValue(entity, out RowEntry entry))
            {
                entry = new RowEntry(entity, EntityState.Unchanged);
                rowEntries[entity] = entry;
            }

            return entry;
        }

        public RowEntry Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entry = GetRowEntry(entity);
            entry.State = EntityState.Added;
            return entry;
        }

        public RowEntry Modify(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entry = GetRowEntry(entity);
            entry.State = EntityState.Modified;
            return entry;
        }

        public RowEntry Remove(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entry = GetRowEntry(entity);
            entry.State = EntityState.Deleted;
            return entry;
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
            foreach (var entry in rowEntries.Values)
            {
                entry.State = EntityState.Detached;
            }

            rowEntries.Clear();
        }

        RowEntry IDbSet.Add(object entity)
        {
            if (entity is T other) return Add(other);
            else throw new InvalidCastException();
        }

        RowEntry IDbSet.Modify(object entity)
        {
            if (entity is T other) return Modify(other);
            else throw new InvalidCastException();
        }

        RowEntry IDbSet.Remove(object entity)
        {
            if (entity is T other) return Remove(other);
            else throw new InvalidCastException();
        }

        void IDbSet.AddRange(IEnumerable<object> entities)
        {
            foreach (var entity in entities)
            {
                if (entity is T other) Add(other);
                else throw new InvalidCastException();
            }
        }

        void IDbSet.ModifyRange(IEnumerable<object> entities)
        {
            foreach (var entity in entities)
            {
                if (entity is T other) Modify(other);
                else throw new InvalidCastException();
            }
        }

        void IDbSet.RemoveRange(IEnumerable<object> entities)
        {
            foreach (var entity in entities)
            {
                if (entity is T other) Remove(other);
                else throw new InvalidCastException();
            }
        }
    }
}
