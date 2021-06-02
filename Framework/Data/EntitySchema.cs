using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Framework.Data
{
    /// <summary>
    /// 数据库映射视图
    /// </summary>
    internal class EntitySchema : IEntitySchema
    {
        private readonly Dictionary<string, ISchemaTable> tables = new Dictionary<string, ISchemaTable>();
        private readonly string name;
        private readonly Type entityType;
        private readonly AccessLevel accessLevel;
        private readonly string? connectKey;
        private DataSaveUsage saveUsage;
        private readonly EntitySchemaAttributes attributes;
        private List<ISchemaColumn>? columns;

        public EntitySchema(string name, Type entityType, AccessLevel accessLevel, string? connectKey, DataSaveUsage saveUsage, EntitySchemaAttributes attributes, IEnumerable<SchemaTable> tables)
        {
            this.name = name;
            this.entityType = entityType;
            this.accessLevel = accessLevel;
            this.connectKey = connectKey;
            this.saveUsage = saveUsage;
            this.attributes = attributes;
            foreach (var table in tables)
            {
                table.Schema = this;
                this.tables.Add(table.Name, table);
            }
        }

        public ISchemaTable this[string name]
        {
            get
            {
                if (!tables.TryGetValue(name, out ISchemaTable table))
                    throw new KeyNotFoundException("not found table name:" + name);

                return table;
            }
        }

        public IReadOnlyCollection<ISchemaTable> Tables => tables.Values;

        public IReadOnlyCollection<ISchemaColumn> Columns
        {
            get
            {
                if (columns == null)
                {
                    var hasPrimaryColumn = false;
                    var columnList = new List<ISchemaColumn>();
                    foreach (var table in tables.Values)
                    {
                        foreach (var column in table.Columns)
                        {
                            if (column.IsPrimary)
                            {
                                if (hasPrimaryColumn)
                                {
                                    continue;
                                }

                                hasPrimaryColumn = true;
                            }

                            columnList.Add(column);
                        }
                    }
                    columns = columnList.OrderByDescending(p => p.IsPrimary).ThenBy(col => col.Order).ToList();
                }
                return columns;
            }
        }

        /// <summary>
        /// 视图名称
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 绑定的实体类型
        /// </summary>
        public Type EntityType => entityType;

        /// <summary>
        /// 访问权限级别
        /// </summary>
        public AccessLevel AccessLevel => accessLevel;

        /// <summary>
        /// 数据库配置连接Key
        /// </summary>
        public string? ConnectKey => connectKey;

        /// <summary>
        /// 保存模式
        /// </summary>
        public DataSaveUsage SaveUsage => saveUsage;

        /// <summary>
        /// 特性
        /// </summary>
        public EntitySchemaAttributes Attributes => attributes;

        public bool TryGetTable(string name, out ISchemaTable? table)
        {
            return tables.TryGetValue(name, out table);
        }

        public bool TryGetColumn(string name, out ISchemaColumn? column)
        {
            foreach (var table in tables.Values)
            {
                foreach (var col in table.Columns)
                {
                    if (col.Name == name)
                    {
                        column = col;
                        return true;
                    }
                }
            }

            column = null;
            return false;
        }
    }
}