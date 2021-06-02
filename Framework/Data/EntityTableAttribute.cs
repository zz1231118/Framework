using System;

namespace Framework.Data
{
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EntityTableAttribute : Attribute
    {
        private Type? reflectedType;
        private string? name;
        private AccessLevel accessLevel = AccessLevel.ReadWrite;
        private DataSaveUsage saveMode = DataSaveUsage.Procedure;
        private EntitySchemaAttributes attributes = EntitySchemaAttributes.Conservatism;

        /// <inheritdoc />
        public EntityTableAttribute()
        { }

        /// <inheritdoc />
        public EntityTableAttribute(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            this.name = name;
        }

        /// <summary>
        /// 获取用于获取该成员的类对象的类型。
        /// </summary>
        public Type ReflectedType
        { 
            get => reflectedType ?? throw new InvalidOperationException("ReflectedType should not be null"); 
            internal set => reflectedType = value;
        }

        /// <summary>
        /// 获取用来声明当前类型参数的类型。
        /// </summary>
        public Type DeclaringType
        {
            get
            {
                if (reflectedType == null)
                    throw new InvalidOperationException("ReflectedType should not be null");

                return reflectedType.DeclaringType;
            }
        }

        /// <summary>
        /// 数据表名
        /// </summary>
        public string Name
        {
            get
            {
                if (name == null)
                {
                    if (reflectedType == null)
                        throw new InvalidOperationException("ReflectedType should not be null");

                    name = reflectedType.Name;
                }
                return name;
            }
            set { name = value; }
        }

        /// <summary>
        /// 连接串的 Key
        /// </summary>
        public string? ConnectKey { get; set; }

        /// <summary>
        /// 访问权限级别
        /// <para>默认：<see cref="AccessLevel.ReadWrite"/></para>
        /// </summary>
        public AccessLevel AccessLevel
        {
            get { return accessLevel; }
            set { accessLevel = value; }
        }

        /// <summary>
        /// 保存模式
        /// <para>默认：<see cref="DataSaveUsage.Procedure"/></para>
        /// </summary>
        public DataSaveUsage SaveUsage
        {
            get { return saveMode; }
            set { saveMode = value; }
        }

        /// <summary>
        /// 特性
        /// <para>默认：<see cref="EntitySchemaAttributes.Conservatism"/></para>
        /// </summary>
        public EntitySchemaAttributes Attributes
        {
            get => attributes;
            set => attributes = value;
        }
    }
}