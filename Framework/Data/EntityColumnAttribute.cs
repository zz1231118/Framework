using System;
using System.Data;
using System.Reflection;
using Framework.Data.Converters;

namespace Framework.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityColumnAttribute : Attribute
    {
        private PropertyInfo? propertyInfo;
        private string? name;
        private DbType? dbType;
        private Type? converterType;
        private IEntityConverter? entityConverter;

        /// <summary>
        /// 属性信息
        /// </summary>
        public PropertyInfo PropertyInfo
        { 
            get => propertyInfo ?? throw new InvalidOperationException("PropertyInfo should not be null"); 
            internal set => propertyInfo = value;
        }

        /// <summary>
        /// 获取成员返回类型
        /// </summary>
        public Type PropertyType
        {
            get
            {
                if (propertyInfo == null)
                    throw new InvalidOperationException("PropertyInfo should not be null");

                return propertyInfo.PropertyType;
            }
        }

        /// <summary>
        /// 获取声明该成员的类
        /// </summary>
        public Type DeclaringType
        {
            get
            {
                if (propertyInfo == null)
                    throw new InvalidOperationException("PropertyInfo should not be null");

                return propertyInfo.DeclaringType;
            }
        }

        /// <summary>
        /// 获取用于获取此成员的此实例的类对象
        /// </summary>
        public Type ReflectedType
        {
            get
            {
                if (propertyInfo == null)
                    throw new InvalidOperationException("PropertyInfo should not be null");

                return propertyInfo.ReflectedType;
            }
        }

        /// <summary>
        /// 是否支持可读
        /// </summary>
        public bool CanRead
        {
            get
            {
                if (propertyInfo == null)
                    throw new InvalidOperationException("PropertyInfo should not be null");

                return propertyInfo.CanRead;
            }
        }

        /// <summary>
        /// 是否支持可写
        /// </summary>
        public bool CanWrite
        {
            get
            {
                if (propertyInfo == null)
                    throw new InvalidOperationException("PropertyInfo should not be null");

                return propertyInfo.CanWrite;
            }
        }

        /// <summary>
        /// 字段名
        /// </summary>
        public string Name
        {
            get
            {
                if (name == null)
                {
                    if (propertyInfo == null)
                        throw new InvalidOperationException("PropertyInfo should not be null");

                    name = propertyInfo.Name;
                }
                return name;
            }
            set { name = value; }
        }

        /// <summary>
        /// 所属的表名
        /// </summary>
        public string? Table { get; set; }

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// 列允许为空
        /// </summary>
        public bool IsNullable { get; set; } = true;

        /// <summary>
        /// 读写模式
        /// </summary>
        public ColumnMode Mode { get; set; } = ColumnMode.ReadWrite;

        /// <summary>
        /// 是否唯一
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// 是否自增
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// 标识种子
        /// </summary>
        public int IdentitySeed { get; set; } = 1;

        /// <summary>
        /// 表示增量
        /// </summary>
        public int Increment { get; set; } = 1;

        /// <summary>
        /// 默认值表达式
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// 禁用或排除数据库取值
        /// </summary>
        public bool Disable { get; set; }

        /// <summary>
        /// 最大长度
        /// </summary>
        public int MaxLength { get; set; } = -1;

        /// <summary>
        /// Db映射类型
        /// </summary>
        public DbType DbType
        {
            get
            {
                if (dbType == null)
                {
                    var converter = GetEntityConverter();
                    var mappingType = converter != null ? converter.GetMappingType(PropertyType) : PropertyType;
                    dbType = EntityConverterManager.GetDefaultDbType(mappingType);
                }

                return dbType.Value;
            }
            set => dbType = value;
        }

        /// <summary>
        /// 转换器
        /// </summary>
        public Type ConverterType
        {
            get
            {
                if (converterType == null)
                {
                    var propertyType = PropertyType.IsEnum ? typeof(Enum) : PropertyType;
                    converterType = EntityConverterManager.GetDefaultEntityConverterType(propertyType);
                }
                return converterType;
            }
            set { converterType = value; }
        }

        private IEntityConverter? GetEntityConverter()
        {
            if (ConverterType != null && entityConverter == null)
            {
                entityConverter = EntityConverterManager.Gain(ConverterType);
            }

            return entityConverter;
        }
    }
}