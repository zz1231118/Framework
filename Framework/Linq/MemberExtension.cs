using System;
using System.Reflection;

namespace Framework.Linq
{
    /// <summary>
    /// MemberInfo 的扩展对象
    /// </summary>
    public static class MemberExtension
    {
        /// <summary>
        /// 指定特性是否定义
        /// </summary>
        /// <param name="source">成员</param>
        /// <param name="attributeType">特性类型</param>
        /// <returns></returns>
        public static bool IsDefined(this MemberInfo source, Type attributeType)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (attributeType == null)
                throw new ArgumentNullException(nameof(attributeType));

            return source.IsDefined(attributeType, true);
        }

        /// <summary>
        /// 指定特性是否定义
        /// </summary>
        /// <typeparam name="T">要搜索的属性类型</typeparam>
        /// <param name="source">成员</param>
        /// <param name="inherit">是否搜索此成员的继承链以查找特性</param>
        /// <returns></returns>
        public static bool IsDefined<T>(this MemberInfo source, bool inherit)
            where T : Attribute
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.IsDefined(typeof(T), inherit);
        }

        /// <summary>
        /// 指定特性是否定义
        /// </summary>
        /// <typeparam name="T">要搜索的属性类型</typeparam>
        /// <param name="source">成员</param>
        /// <returns></returns>
        public static bool IsDefined<T>(this MemberInfo source)
            where T : Attribute
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.IsDefined(typeof(T), true);
        }
    }
}
