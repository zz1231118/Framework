using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.Linq;

namespace Framework.Data
{
    /// <summary>
    /// 实体架构信息管理器
    /// </summary>
    public static class EntitySchemaManager
    {
        private readonly static ConcurrentDictionary<string, IEntitySchema> entitySchemas = new ConcurrentDictionary<string, IEntitySchema>();

        /// <summary>
        /// 实体架构信息集合
        /// </summary>
        public static ICollection<IEntitySchema> Schemas => entitySchemas.Values;

        private static EntityTableAttribute? GetEntityTableAttribute(Type type)
        {
            var attribute = type.GetCustomAttribute<EntityTableAttribute>();
            if (attribute != null) attribute.ReflectedType = type;
            return attribute;
        }

        private static List<EntityColumnAttribute> GetEntityColumnAttributes(Type type)
        {
            var columns = new List<EntityColumnAttribute>();
            foreach (var propertyInfo in ReflexHelper.GetPropertys(type))
            {
                var attribute = propertyInfo.GetCustomAttribute<EntityColumnAttribute>();
                if (attribute != null)
                {
                    attribute.PropertyInfo = propertyInfo;
                    columns.Add(attribute);
                }
            }
            return columns;
        }

        /// <summary>
        /// 加载实体程序集
        /// </summary>
        public static void LoadAssemblys(params Assembly[] assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(p => p.IsDefined<EntityTableAttribute>()))
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
            
            var contract = GetEntityTableAttribute(entityType);
            if (contract == null)
            {
                //EntityTableAttribute not found
                throw new ArgumentException($"type:{entityType.FullName} not found EntityTableAttribute!");
            }

            var orderNum = 0;
            var columnList = new List<ISchemaColumn>();
            foreach (var entityColumn in GetEntityColumnAttributes(entityType))
            {
                if (entityColumn.Mode.HasFlag(ColumnMode.ReadOnly) && !entityColumn.CanWrite)
                {
                    throw new InvalidOperationException($"{entityType.FullName}.{entityColumn.Name} not to writer");
                }
                if (entityColumn.Mode.HasFlag(ColumnMode.WriteOnly) && !entityColumn.CanRead)
                {
                    throw new InvalidOperationException($"{entityType.FullName}.{entityColumn.Name} not to reader");
                }

                var column = new SchemaColumn(++orderNum,
                    entityColumn.Table ?? contract.Name,
                    entityColumn.Name,
                    entityColumn.MaxLength,
                    entityColumn.DbType,
                    entityColumn.PropertyInfo,
                    entityColumn.PropertyType,
                    entityColumn.DeclaringType,
                    entityColumn.ReflectedType,
                    entityColumn.ConverterType,
                    entityColumn.Mode,
                    entityColumn.CanRead,
                    entityColumn.CanWrite,
                    entityColumn.IsNullable,
                    entityColumn.IsPrimary,
                    entityColumn.IsIdentity,
                    entityColumn.IdentitySeed,
                    entityColumn.Increment,
                    entityColumn.DefaultValue);
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

            var entitySchema = new EntitySchema(contract.Name, 
                entityType, 
                contract.AccessLevel, 
                contract.ConnectKey, 
                contract.SaveUsage, 
                contract.Attributes, 
                schemaTableList);
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

        /// <summary>
        /// 指定实体架构信息是否存在
        /// </summary>
        /// <param name="fullName">指定实体全名</param>
        /// <returns></returns>
        public static bool Contains(string fullName)
        {
            return entitySchemas.ContainsKey(fullName);
        }

        /// <summary>
        /// 指定实体架构信息是否存在
        /// </summary>
        /// <param name="entityType">指定实体类型</param>
        /// <returns></returns>
        public static bool Contains(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            return entitySchemas.ContainsKey(entityType.FullName);
        }

        /// <summary>
        /// 从管理器中移除指定实体架构信息
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static bool Remove(IEntitySchema schema)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            return entitySchemas.TryRemove(schema.EntityType.FullName, out _);
        }

        /// <summary>
        /// 获取指定实体架构信息
        /// </summary>
        /// <param name="fullName">欲获取架构信息的实体全名</param>
        /// <param name="throwOnError">如果不存在是否抛出异常</param>
        /// <returns></returns>
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

        /// <summary>
        /// 获取指定实体架构信息
        /// </summary>
        /// <param name="entityType">欲获取架构信息的实体类型</param>
        /// <param name="throwOnError">如果不存在是否抛出异常</param>
        /// <returns></returns>
        public static IEntitySchema GetSchema(Type entityType, bool throwOnError = false)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            return GetSchema(entityType.FullName, throwOnError);
        }

        /// <summary>
        /// 获取指定实体架构信息
        /// </summary>
        /// <typeparam name="T">欲获取架构信息的实体类型</typeparam>
        /// <param name="throwOnError">如果不存在是否抛出异常</param>
        /// <returns></returns>
        public static IEntitySchema GetSchema<T>(bool throwOnError = false)
        {
            return GetSchema(typeof(T).FullName, throwOnError);
        }

        /// <summary>
        /// 尝试获取指定实体架构信息
        /// </summary>
        /// <param name="fullName">欲获取架构信息的实体全名</param>
        /// <param name="schema">如果指定实体架构信息存在，则返回，否则返回null。</param>
        /// <returns></returns>
        public static bool TryGetSchema(string fullName, out IEntitySchema schema)
        {
            if (fullName == null)
                throw new ArgumentNullException(nameof(fullName));
            if (fullName == string.Empty)
                throw new ArgumentException(nameof(fullName));

            return entitySchemas.TryGetValue(fullName, out schema);
        }

        /// <summary>
        /// 尝试获取指定实体架构信息
        /// </summary>
        /// <typeparam name="T">欲获取架构信息的实体类型</typeparam>
        /// <param name="schema">如果指定实体架构信息存在，则返回，否则返回null。</param>
        /// <returns></returns>
        public static bool TryGetSchema<T>(out IEntitySchema schema)
        {
            return TryGetSchema(typeof(T).FullName, out schema);
        }

        /// <summary>
        /// 尝试获取指定实体架构信息
        /// </summary>
        /// <param name="entityType">欲获取架构信息的实体类型</param>
        /// <param name="schema">如果指定实体架构信息存在，则返回，否则返回null。</param>
        /// <returns></returns>
        public static bool TryGetSchema(Type entityType, out IEntitySchema schema)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            return TryGetSchema(entityType.FullName, out schema);
        }
    }
}