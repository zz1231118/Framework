using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Framework.Data;
using Framework.Data.Command;
using Framework.Data.Expressions;
using Framework.JavaScript;
using Framework.Linq;

namespace ConsoleApp1
{
    public abstract class RowAdapter
    {
        private static readonly ConcurrentDictionary<Type, long> _mkv = new ConcurrentDictionary<Type, long>();
        private static readonly Func<Type, long, long> _mUpdateFactory = (key, old) => old + 1;
        private SaveUsage _saveUsage;

        [JsonMember]
        [EntityColumn(IsPrimary = true)]
        public virtual long ID { get; set; }

        public static void LoadPrimary(IEntitySchema entitySchema)
        {
            if (entitySchema == null)
                throw new ArgumentNullException(nameof(entitySchema));
            if (!DbConnectionManager.TryGet(entitySchema.ConnectKey, out DbConnectionProvider connectionProvider))
                throw new InvalidOperationException("not found DbProvider:" + entitySchema.ConnectKey);

            var str = string.Format("Select IsNull(Max([ID]), 0) From [{0}]", entitySchema.Name);
            var obj = connectionProvider.ExecuteScalar(str);
            var maxPrimary = Convert.ToInt64(obj);
            _mkv[entitySchema.EntityType] = maxPrimary;
        }
        public static long NewID<T>()
        {
            return _mkv.AddOrUpdate(typeof(T), 1, _mUpdateFactory);
        }
        public static T Create<T>()
            where T : RowAdapter, new()
        {
            var obj = Activator.CreateInstance<T>();
            obj._saveUsage = SaveUsage.Insert;
            obj.ID = RowAdapter.NewID<T>();
            obj.CreateSuccess();
            obj.Initialize();
            return obj;
        }
        public static List<T> Load<T>(DbConnectionProvider connectionProvider, IDbCommandStruct<T> commandStruct)
            where T : RowAdapter, new()
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (commandStruct == null)
                throw new ArgumentNullException(nameof(commandStruct));

            var rows = connectionProvider.Select<T>(commandStruct);
            foreach (var row in rows)
            {
                row._saveUsage = SaveUsage.Update;
                row.Initialize();
            }
            return rows;
        }
        public static List<T> Load<T>(DbConnectionProvider connectionProvider, Expression<Func<T, bool>> condition = null)
            where T : RowAdapter, new()
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));

            var view = EntitySchemaManager.GetSchema<T>(true);
            var validColumns = view.Columns.Select(p => SqlExpression.Member(p.Name));
            var commandStruct = connectionProvider.CreateCommand<T>(view.Name, DbCommandMode.Select, validColumns);
            if (condition != null)
            {
                commandStruct.Where(condition);
            }
            return Load<T>(connectionProvider, commandStruct);
        }
        public static List<T> Load<T>(IDbCommandStruct<T> commandStruct)
            where T : RowAdapter, new()
        {
            if (commandStruct == null)
                throw new ArgumentNullException(nameof(commandStruct));

            if (!EntitySchemaManager.TryGetSchema<T>(out IEntitySchema view))
                throw new ArgumentException(string.Format("not found {0}:{1}", nameof(IEntitySchema), typeof(T).FullName));
            if (!DbConnectionManager.TryGet(view.ConnectKey, out DbConnectionProvider connectionProvider))
                throw new InvalidOperationException("not found DbProvider:" + view.ConnectKey);

            return Load<T>(connectionProvider, commandStruct);
        }
        public static List<T> Load<T>(Expression<Func<T, bool>> condition = null)
            where T : RowAdapter, new()
        {
            if (!EntitySchemaManager.TryGetSchema<T>(out IEntitySchema view))
                throw new ArgumentException(string.Format("not found {0}:{1}", nameof(IEntitySchema), typeof(T).FullName));
            if (!DbConnectionManager.TryGet(view.ConnectKey, out DbConnectionProvider connectionProvider))
                throw new InvalidOperationException("not found DbProvider:" + view.ConnectKey);

            return Load<T>(connectionProvider, condition);
        }
        public static T LoadFirst<T>(Expression<Func<T, bool>> condition = null)
            where T : RowAdapter, new()
        {
            return Load<T>(condition).First();
        }
        public static T LoadFirstOrDefault<T>(Expression<Func<T, bool>> condition = null)
            where T : RowAdapter, new()
        {
            return Load<T>(condition).FirstOrDefault();
        }
        public static T LoadSingle<T>(Expression<Func<T, bool>> condition = null)
            where T : RowAdapter, new()
        {
            return Load<T>(condition).Single();
        }
        public static T LoadSingleOrDefault<T>(Expression<Func<T, bool>> condition = null)
            where T : RowAdapter, new()
        {
            return Load<T>(condition).SingleOrDefault();
        }

        public void Save()
        {
            if (!EntitySchemaManager.TryGetSchema(GetType(), out IEntitySchema view))
                throw new ArgumentException();
            if (!DbConnectionManager.TryGet(view.ConnectKey, out DbConnectionProvider connectionProvider))
                throw new InvalidOperationException("not found DbProvider:" + view.ConnectKey);

            switch (_saveUsage)
            {
                case SaveUsage.Unknown:
                    throw new InvalidOperationException("Unknown SaveUsage");
                case SaveUsage.Update:
                    connectionProvider.Update(new object[] { this });
                    break;
                case SaveUsage.Insert:
                    connectionProvider.Insert(new object[] { this });
                    break;
            }
        }

        protected virtual void CreateSuccess()
        { }
        protected virtual void Initialize()
        { }
    }

    public enum SaveUsage
    {
        Unknown,
        Update,
        Insert,
    }
}
