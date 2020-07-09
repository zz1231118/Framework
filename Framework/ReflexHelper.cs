using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Framework
{
    public static class ReflexHelper
    {
        internal static IEnumerable<PropertyInfo> GetPropertys(Type type)
        {
            var depth = 0;
            var nextType = type;
            var bottomType = typeof(object);
            var dictionary = new Dictionary<Type, int>();

            do
            {
                dictionary[nextType] = depth++;
                nextType = nextType.BaseType;
            } while (nextType != bottomType);

            var bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return type.GetProperties(bindingAttr).OrderByDescending(p => dictionary[p.DeclaringType]);
        }

        internal static T InternalGetCustomAttribute<T>(MemberInfo element, bool inherit = false)
            where T : Attribute
        {
            var attributes = element.GetCustomAttributes(typeof(T), inherit);
            return attributes.Length == 0 ? null : attributes[0] as T;
        }

        internal static T[] InternalGetCustomAttributes<T>(MemberInfo element, bool inherit = false)
            where T : Attribute
        {
            var attributes = element.GetCustomAttributes(typeof(T), inherit);
            return attributes.Cast<T>().ToArray();
        }

        internal static T InternalGetTypeAttribute<T>(Type type, bool inherit = false)
            where T : TypeAttribute
        {
            var attribute = InternalGetCustomAttribute<T>(type, inherit);
            if (attribute != null) attribute.ReflectedType = type;

            return attribute;
        }

        internal static T InternalGetPropertyAttribute<T>(PropertyInfo property, bool inherit = false)
            where T : PropertyAttribute
        {
            var attribute = InternalGetCustomAttribute<T>(property, inherit);
            if (attribute != null) attribute.PropertyInfo = property;

            return attribute;
        }

        internal static T[] InternalGetPropertyAttributes<T>(Type type, bool inherit = false)
            where T : PropertyAttribute
        {
            var result = new List<T>();
            var bindAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var property in type.GetProperties(bindAttr))
            {
                var attribute = InternalGetPropertyAttribute<T>(property, inherit);
                if (attribute != null) result.Add(attribute);
            }
            return result.ToArray();
        }

        public static T GetTypeAttribute<T>(this Type type, bool inherit = false)
            where T : TypeAttribute
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return InternalGetTypeAttribute<T>(type, inherit);
        }

        public static T GetPropertyAttribute<T>(this PropertyInfo property, bool inherit = false)
            where T : PropertyAttribute
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            return InternalGetPropertyAttribute<T>(property, inherit);
        }

        public static T[] GetPropertyAttributes<T>(this Type type, bool inherit = false)
            where T : PropertyAttribute
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return InternalGetPropertyAttributes<T>(type, inherit);
        }
    }
}
