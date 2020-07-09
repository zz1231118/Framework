using System;

namespace Framework
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class TypeAttribute : Attribute
    {
        /// <summary>
        /// 获取用于获取该成员的类对象的类型。
        /// </summary>
        public Type ReflectedType { get; internal set; }

        /// <summary>
        /// 获取用来声明当前类型参数的类型。
        /// </summary>
        public Type DeclaringType
        {
            get
            {
                if (ReflectedType == null)
                    throw new InvalidOperationException("ReflectedType is null!");

                return ReflectedType.DeclaringType;
            }
        }
    }
}