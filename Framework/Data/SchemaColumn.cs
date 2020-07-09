using System;
using System.Data;
using System.Reflection;
using Framework.Data.Converters;

namespace Framework.Data
{
    /// <summary>
    /// 实体架构的列信息
    /// </summary>
    internal class SchemaColumn : ISchemaColumn
    {
        private IEntityConverter entityConverter;

        public SchemaColumn()
        { }

        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 最大长度
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// Db映射类型
        /// </summary>
        public DbType DbType { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// 属性信息
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// 属性类型
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// 获取声明该成员的类
        /// </summary>
        public Type DeclaringType { get; set; }

        /// <summary>
        /// 获取用于获取此成员的此实例的类对象
        /// </summary>
        public Type ReflectedType { get; set; }

        /// <summary>
        /// 是否支持可读
        /// </summary>
        public bool CanRead { get; set; }

        /// <summary>
        /// 是否支持可写
        /// </summary>
        public bool CanWrite { get; set; }

        /// <summary>
        /// 列允许为空
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// 读写模式
        /// </summary>
        public ColumnModel Model { get; set; }

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// 是否自增
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// 标识种子
        /// </summary>
        public int IdentitySeed { get; set; }

        /// <summary>
        /// 表示增量
        /// </summary>
        public int Increment { get; set; }

        /// <summary>
        /// 默认值表达式
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// 实体转换类型
        /// </summary>
        public Type ConvertType { get; set; }

        protected bool TryGetEntityConverter(out IEntityConverter converter)
        {
            if (ConvertType == null)
            {
                converter = null;
                return false;
            }
            if (entityConverter != null)
            {
                converter = entityConverter;
                return true;
            }

            entityConverter = EntityConverterSet.Gain(ConvertType);
            converter = entityConverter;
            return true;
        }

        public void SetValue(object obj, object value)
        {
            if (PropertyInfo == null)
                throw new InvalidOperationException("PropertyInfo is null!");

            if (TryGetEntityConverter(out IEntityConverter converter))
            {
                value = converter.ConvertTo(value, PropertyType);
            }
            else if (value != null)
            {
                if (typeof(IConvertible).IsAssignableFrom(PropertyType))
                {
                    if (value.GetType() != PropertyType)
                        value = Convert.ChangeType(value, PropertyType);
                }
            }

            PropertyInfo.SetValue(obj, value, null);
        }

        public object GetValue(object obj)
        {
            if (PropertyInfo == null)
                throw new InvalidOperationException("PropertyInfo is null!");

            var result = PropertyInfo.GetValue(obj, null);
            if (result == null) return null;
            if (TryGetEntityConverter(out IEntityConverter converter))
            {
                var tType = EntityUtils.ConvertToType(PropertyType);
                result = converter.ConvertFrom(result, tType);
            }

            return result;
        }
    }
}