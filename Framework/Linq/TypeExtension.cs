using System;
using System.Runtime.CompilerServices;

namespace Framework.Linq
{
    /// <summary>
    /// Type 的扩展对象
    /// </summary>
    public static class TypeExtension
    {
        /// <summary>
        /// 是否为封装的可空的值类型
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static bool IsNullable(this Type nullableType)
        {
            if (nullableType == null)
                throw new ArgumentNullException(nameof(nullableType));

            if (nullableType.IsGenericType && !nullableType.IsGenericTypeDefinition)
            {
                Type genericTypeDefinition = nullableType.GetGenericTypeDefinition();
                return object.ReferenceEquals(genericTypeDefinition, typeof(Nullable<>));
            }
            return false;
        }

        /// <summary>
        /// 是否是匿名类
        /// </summary>
        public static bool IsAnonymousType(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!type.IsGenericType)
                return false;
            if (!type.IsNotPublic)
                return false;
            if (!Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false))
                return false;

            return type.Name.Contains("AnonymousType");
        }
    }
}