using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Framework.Data.Converters;

namespace Framework.Data
{
    /// <summary>
    /// 实体架构的列信息
    /// </summary>
    internal class SchemaColumn : ISchemaColumn
    {
        private readonly int order;
        private readonly string table;
        private readonly string name;
        private readonly int maxLength;
        private readonly DbType dbType;
        private readonly PropertyInfo propertyInfo;
        private readonly Type propertyType;
        private readonly Type declaringType;
        private readonly Type reflectedType;
        private readonly Type converterType;
        private readonly ColumnMode mode;
        private readonly bool canRead;
        private readonly bool canWrite;
        private readonly bool isNullable;
        private readonly bool isPrimary;
        private readonly bool isIdentity;
        private readonly int identitySeed;
        private readonly int increment;
        private readonly string? defaultValue;
        private IEntityConverter? entityConverter;

        public SchemaColumn(int order, string table, string name, int maxLength, DbType dbType,
            PropertyInfo propertyInfo, Type propertyType, Type declaringType, Type reflectedType, Type converterType,
            ColumnMode mode, bool canRead, bool canWrite, bool isNullable, bool isPrimary,
            bool isIdentity, int identitySeed, int increment, string? defaultValue)
        {
            this.order = order;
            this.table = table;
            this.name = name;
            this.maxLength = maxLength;
            this.dbType = dbType;
            this.propertyInfo = propertyInfo;
            this.propertyType = propertyType;
            this.declaringType = declaringType;
            this.reflectedType = reflectedType;
            this.converterType = converterType;
            this.mode = mode;
            this.canRead = canRead;
            this.canWrite = canWrite;
            this.isNullable = isNullable;
            this.isPrimary = isPrimary;
            this.isIdentity = isIdentity;
            this.identitySeed = identitySeed;
            this.increment = increment;
            this.defaultValue = defaultValue;
        }

        /// <summary>
        /// 排序
        /// </summary>
        public int Order => order;

        /// <summary>
        /// 表名
        /// </summary>
        public string Table => table;

        /// <summary>
        /// 列名
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 最大长度
        /// </summary>
        public int MaxLength => maxLength;

        /// <summary>
        /// Db映射类型
        /// </summary>
        public DbType DbType => dbType;

        /// <summary>
        /// 属性信息
        /// </summary>
        public PropertyInfo PropertyInfo => propertyInfo;

        /// <summary>
        /// 属性类型
        /// </summary>
        public Type PropertyType => propertyType;

        /// <summary>
        /// 获取声明该成员的类
        /// </summary>
        public Type DeclaringType => declaringType;

        /// <summary>
        /// 获取用于获取此成员的此实例的类对象
        /// </summary>
        public Type ReflectedType => reflectedType;

        /// <summary>
        /// 实体转换类型
        /// </summary>
        public Type ConverterType => converterType;

        /// <summary>
        /// 读写模式
        /// </summary>
        public ColumnMode Mode => mode;

        /// <summary>
        /// 是否支持可读
        /// </summary>
        public bool CanRead => canRead;

        /// <summary>
        /// 是否支持可写
        /// </summary>
        public bool CanWrite => canWrite;

        /// <summary>
        /// 列允许为空
        /// </summary>
        public bool IsNullable => isNullable;

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimary => isPrimary;

        /// <summary>
        /// 是否自增
        /// </summary>
        public bool IsIdentity => isIdentity;

        /// <summary>
        /// 标识种子
        /// </summary>
        public int IdentitySeed => identitySeed;

        /// <summary>
        /// 表示增量
        /// </summary>
        public int Increment => increment;

        /// <summary>
        /// 默认值表达式
        /// </summary>
        public string? DefaultValue => defaultValue;

        private bool TryGetEntityConverter(out IEntityConverter? converter)
        {
            if (ConverterType == null)
            {
                converter = null;
                return false;
            }
            if (entityConverter != null)
            {
                converter = entityConverter;
                return true;
            }

            entityConverter = EntityConverterManager.Gain(ConverterType);
            converter = entityConverter;
            return true;
        }

        public void SetValue(object obj, object value)
        {
            if (PropertyInfo == null)
                throw new InvalidOperationException("PropertyInfo is null!");

            if (TryGetEntityConverter(out IEntityConverter? converter))
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

        public object? GetValue(object obj)
        {
            if (PropertyInfo == null)
                throw new InvalidOperationException("PropertyInfo is null!");

            var result = PropertyInfo.GetValue(obj, null);
            if (result == null) return null;
            if (TryGetEntityConverter(out IEntityConverter? converter))
            {
                var tType = EntityUtils.ConvertToType(PropertyType);
                result = converter.ConvertFrom(result, tType);
            }

            return result;
        }
    }
}