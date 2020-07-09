using System;
using System.Collections.Generic;

namespace Framework.Data
{
    public interface ISchemaTable
    {
        ISchemaColumn this[string name] { get; }

        IEntitySchema Schema { get; }

        /// <summary>
        /// 列集合
        /// </summary>
        IReadOnlyCollection<ISchemaColumn> Columns { get; }

        /// <summary>
        /// 索引集合
        /// </summary>
        IReadOnlyCollection<SchemaIndex> Indices { get; }

        /// <summary>
        /// 实体名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 绑定的实体类型
        /// </summary>
        Type EntityType { get; }

        bool TryGetColumn(string name, out ISchemaColumn column);
    }
}