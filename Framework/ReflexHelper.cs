using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Framework
{
    internal static class ReflexHelper
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
    }
}
