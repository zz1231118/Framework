using System;
using System.Data;
using System.Reflection;
using Framework.Data.Converters;

namespace Framework.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityColumnAttribute : PropertyAttribute
    {
        private string name;
        private DbType? dbType;
        private Type converterType;
        private IEntityConverter entityConverter;

        /// <summary>
        /// 字段名
        /// </summary>
        public string Name
        {
            get
            {
                if (name == null)
                {
                    if (PropertyInfo == null)
                        throw new InvalidOperationException("PropertyInfo is null!");

                    name = PropertyInfo.Name;
                }
                return name;
            }
            set { name = value; }
        }

        /// <summary>
        /// 所属的表名
        /// </summary>
        public string Table { get; set; }

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
        public ColumnModel Model { get; set; } = ColumnModel.ReadWrite;

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
        public string DefaultValue { get; set; }

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
                    dbType = EntityConverterSet.GetDefaultDbType(mappingType);
                }

                return dbType.Value;
            }
            set => dbType = value;
        }

        /// <summary>
        /// 转换器
        /// </summary>
        public Type ConvertType
        {
            get
            {
                if (converterType == null)
                {
                    var propertyType = PropertyType.IsEnum ? typeof(Enum) : PropertyType;
                    converterType = EntityConverterSet.GetDefaultEntityConverterType(propertyType);
                }
                return converterType;
            }
            set { converterType = value; }
        }

        private IEntityConverter GetEntityConverter()
        {
            if (ConvertType != null && entityConverter == null)
            {
                entityConverter = EntityConverterSet.Gain(ConvertType);
            }

            return entityConverter;
        }

        public override void SetValue(object obj, object value, object[] index)
        {
            var converter = GetEntityConverter();
            if (converter != null)
            {
                value = converter.ConvertTo(value, PropertyType);
            }
            if (value != null)
            {
                if (typeof(IConvertible).IsAssignableFrom(PropertyType))
                {
                    if (value.GetType() != PropertyType)
                    {
                        value = Convert.ChangeType(value, PropertyType);
                    }
                }
            }
            base.SetValue(obj, value, index);
        }

        public override object GetValue(object obj, object[] index)
        {
            var value = base.GetValue(obj, index);
            var converter = GetEntityConverter();
            if (converter != null)
            {
                var tType = EntityUtils.ConvertToType(PropertyType);
                value = converter.ConvertFrom(value, tType);
            }
            return value;
        }
    }
}