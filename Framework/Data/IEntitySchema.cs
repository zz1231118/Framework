using System;
using System.Collections.Generic;

namespace Framework.Data
{
    public interface IEntitySchema
    {
        ISchemaTable this[string name] { get; }

        /// <summary>
        /// 表集合
        /// </summary>
        IReadOnlyCollection<ISchemaTable> Tables { get; }

        /// <summary>
        /// 列集合
        /// </summary>
        IReadOnlyCollection<ISchemaColumn> Columns { get; }

        /// <summary>
        /// 视图名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 绑定的实体类型
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// 访问权限级别
        /// </summary>
        AccessLevel AccessLevel { get; }

        /// <summary>
        /// 数据库配置连接Key
        /// </summary>
        string ConnectKey { get; }

        /// <summary>
        /// 数据交换模式
        /// </summary>
        DataSaveUsage SaveUsage { get; }

        /// <summary>
        /// 特性
        /// </summary>
        EntitySchemaAttributes Attributes { get; }

        bool TryGetTable(string name, out ISchemaTable table);

        bool TryGetColumn(string name, out ISchemaColumn column);
    }
}