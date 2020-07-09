using System;
using System.Data;

namespace Framework.Data
{
    public interface ISchemaColumn
    {
        /// <summary>
        /// 排序
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 列名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 最大长度
        /// </summary>
        int MaxLength { get; }

        /// <summary>
        /// Db映射类型
        /// </summary>
        DbType DbType { get; }

        /// <summary>
        /// 表名
        /// </summary>
        string Table { get; }

        /// <summary>
        /// 属性类型
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// 获取声明该成员的类
        /// </summary>
        Type DeclaringType { get; }

        /// <summary>
        /// 获取用于获取此成员的此实例的类对象
        /// </summary>
        Type ReflectedType { get; }

        /// <summary>
        /// 是否支持可读
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// 是否支持可写
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// 列允许为空
        /// </summary>
        bool IsNullable { get; }

        /// <summary>
        /// 读写模式
        /// </summary>
        ColumnModel Model { get; }

        /// <summary>
        /// 是否主键
        /// </summary>
        bool IsPrimary { get; }

        /// <summary>
        /// 是否自增
        /// </summary>
        bool IsIdentity { get; }

        /// <summary>
        /// 标识种子
        /// </summary>
        int IdentitySeed { get; }

        /// <summary>
        /// 表示增量
        /// </summary>
        int Increment { get; }

        /// <summary>
        /// 默认值表达式
        /// </summary>
        string DefaultValue { get; }

        /// <summary>
        /// 实体转换类型
        /// </summary>
        Type ConvertType { get; }

        /// <summary>
        /// 获取值
        /// </summary>
        object GetValue(object obj);

        /// <summary>
        /// 设置值
        /// </summary>
        void SetValue(object obj, object value);
    }
}