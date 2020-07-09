using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Data
{
    /// <summary>
    /// 数据库映射表
    /// </summary>
    internal class SchemaTable : ISchemaTable
    {
        private readonly Dictionary<string, ISchemaColumn> columns;
        private readonly Dictionary<string, SchemaIndex> indices;

        public SchemaTable(IEnumerable<ISchemaColumn> columns, IEnumerable<SchemaIndex> indices)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (indices == null)
                throw new ArgumentNullException(nameof(indices));

            this.columns = columns.ToDictionary(p => p.Name);
            this.indices = indices.ToDictionary(p => p.Name);
        }

        public ISchemaColumn this[string name]
        {
            get
            {
                if (!columns.TryGetValue(name, out ISchemaColumn column))
                    throw new KeyNotFoundException("not found column name:" + name);

                return column;
            }
        }

        public IEntitySchema Schema { get; set; }

        /// <summary>
        /// 列集合
        /// </summary>
        public IReadOnlyCollection<ISchemaColumn> Columns => columns.Values;

        /// <summary>
        /// 索引集合
        /// </summary>
        public IReadOnlyCollection<SchemaIndex> Indices => indices.Values;

        /// <summary>
        /// 实体名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 绑定的实体类型
        /// </summary>
        public Type EntityType { get; set; }

        public bool TryGetColumn(string name, out ISchemaColumn column)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return columns.TryGetValue(name, out column);
        }
    }
}