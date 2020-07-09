using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Framework.Data
{
    public static class EntitySchemaManager
    {
        private readonly static ConcurrentDictionary<string, IEntitySchema> entitySchemas = new ConcurrentDictionary<string, IEntitySchema>();

        public static ICollection<IEntitySchema> Schemas => entitySchemas.Values;

        /// <summary>
        /// 加载实体程序集
        /// </summary>
        public static void LoadAssemblys(params Assembly[] assemblys)
        {
            if (assemblys == null)
                throw new ArgumentNullException(nameof(assemblys));

            foreach (var assembly in assemblys)
            {
                foreach (var type in assembly.GetTypes().Where(p => p.GetCustomAttribute<EntityTableAttribute>() != null))
                {
                    LoadEntity(type);
                }
            }
        }

        /// <summary>
        /// 初始化架构信息
        /// </summary>
        public static IEntitySchema LoadEntity(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            var entityTable = EntityUtils.GetEntityTableAttribute(entityType);
            if (entityTable == null)
            {
                //EntityTableAttribute not found
                throw new ArgumentException(string.Format("type:{0} not found EntityTableAttribute!", entityType.FullName));
            }

            var orderNum = 0;
            var columnList = new List<ISchemaColumn>();
            foreach (var entityColumn in EntityUtils.GetEntityColumnAttributes(entityType))
            {
                if (entityColumn.Model.HasFlag(ColumnModel.ReadOnly) && !entityColumn.CanWrite)
                {
                    throw new ArgumentException($"{entityType.FullName}.{entityColumn.Name} not to writer");
                }
                if (entityColumn.Model.HasFlag(ColumnModel.WriteOnly) && !entityColumn.CanRead)
                {
                    throw new ArgumentException($"{entityType.FullName}.{entityColumn.Name} not to reader");
                }

                var column = new SchemaColumn();
                column.Order = ++orderNum;
                column.PropertyInfo = entityColumn.PropertyInfo;
                column.PropertyType = entityColumn.PropertyType;
                column.Name = entityColumn.Name;
                column.Table = entityColumn.Table ?? entityTable.Name;
                column.IsIdentity = entityColumn.IsIdentity;
                column.IdentitySeed = entityColumn.IdentitySeed;
                column.Increment = entityColumn.Increment;
                column.IsPrimary = entityColumn.IsPrimary;
                column.IsNullable = entityColumn.IsNullable;
                column.Model = entityColumn.Model;
                column.DefaultValue = entityColumn.DefaultValue;
                column.DeclaringType = entityColumn.DeclaringType;
                column.ReflectedType = entityColumn.ReflectedType;
                column.DbType = entityColumn.DbType;
                column.MaxLength = entityColumn.MaxLength;
                column.CanRead = entityColumn.CanRead;
                column.CanWrite = entityColumn.CanWrite;
                column.ConvertType = entityColumn.ConvertType;

                columnList.Add(column);
            }

            var primaryColumnList = columnList.Where(p => p.IsPrimary).ToList();
            if (primaryColumnList.Count > 1)
            {
                throw new ArgumentException(string.Format("type name:{0} primary count > 1", entityType.Name));
            }
            primaryColumnList.ForEach(p => columnList.Remove(p));
            var tableColumnGroupList = columnList.GroupBy(p => p.Table).ToList();
            if (tableColumnGroupList.Count > 1 && primaryColumnList.Count == 0)
            {
                throw new ArgumentException(string.Format("type name:{0} no key view!", entityType.Name));
            }
            var indices = new List<SchemaIndex>();
            foreach (var metadata in entityType.GetCustomAttributes<EntityIndexAttribute>())
            {
                var index = new SchemaIndex();
                index.Name = string.Format("{0}_{1}", entityType.Name, string.Join("-", metadata.Columns));
                index.Category = metadata.Category;
                index.Unique = metadata.Unique;
                index.Columns = columnList.Where(p => metadata.Columns.Contains(p.Name)).ToList();
                indices.Add(index);
            }

            var schemaTableList = new List<SchemaTable>();
            foreach (var tableColumnGroup in tableColumnGroupList)
            {
                var columns = new List<ISchemaColumn>();
                columns.AddRange(primaryColumnList);
                columns.AddRange(tableColumnGroup);

                var schemaTable = new SchemaTable(columns, indices);
                schemaTable.Name = tableColumnGroup.Key;
                schemaTable.EntityType = entityType;
                schemaTableList.Add(schemaTable);
            }

            var entitySchema = new EntitySchema(schemaTableList);
            entitySchema.EntityType = entityType;
            entitySchema.AccessLevel = entityTable.AccessLevel;
            entitySchema.Name = entityTable.Name;
            entitySchema.ConnectKey = entityTable.ConnectKey;
            entitySchema.SaveUsage = entityTable.SaveUsage;
            entitySchema.Attributes = entityTable.Attributes;
            entitySchemas[entityType.FullName] = entitySchema;
            return entitySchema;
        }

        /// <summary>
        /// 初始化架构信息
        /// </summary>
        public static IEntitySchema LoadEntity<T>()
            where T : class
        {
            return LoadEntity(typeof(T));
        }

        public static bool Contains(string fullName)
        {
            return entitySchemas.ContainsKey(fullName);
        }

        public static bool Contains(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            return entitySchemas.ContainsKey(entityType.FullName);
        }

        public static bool Remove(IEntitySchema schema)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            return entitySchemas.TryRemove(schema.EntityType.FullName, out _);
        }

        public static IEntitySchema GetSchema(string fullName, bool throwOnError = false)
        {
            if (fullName == null)
                throw new ArgumentNullException(nameof(fullName));
            if (fullName == string.Empty)
                throw new ArgumentException(nameof(fullName));

            if (!TryGetSchema(fullName, out IEntitySchema table) && throwOnError)
            {
                throw new KeyNotFoundException(fullName);
            }

            return table;
        }

        public static IEntitySchema GetSchema(Type entityType, bool throwOnError = false)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            return GetSchema(entityType.FullName, throwOnError);
        }

        public static IEntitySchema GetSchema<T>(bool throwOnError = false)
        {
            return GetSchema(typeof(T).FullName, throwOnError);
        }

        public static bool TryGetSchema<T>(out IEntitySchema schema)
        {
            return TryGetSchema(typeof(T).FullName, out schema);
        }

        public static bool TryGetSchema(Type entityType, out IEntitySchema schema)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            return TryGetSchema(entityType.FullName, out schema);
        }

        public static bool TryGetSchema(string fullName, out IEntitySchema schema)
        {
            if (fullName == null)
                throw new ArgumentNullException(nameof(fullName));
            if (fullName == string.Empty)
                throw new ArgumentException(nameof(fullName));

            return entitySchemas.TryGetValue(fullName, out schema);
        }
    }
}