using System;
using System.Collections.Generic;
using System.Reflection;

namespace Framework.Data.Entry
{
    internal static class TypeSystem
    {
        private static Type FindIEnumerable(Type sequenceType)
        {
            // Ignores "terminal" primitive types in the EDM although they may implement IEnumerable<>
            if (sequenceType == null || sequenceType == typeof(string) || sequenceType == typeof(byte[]))
            {
                return null;
            }
            if (sequenceType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(sequenceType.GetElementType());
            }
            if (sequenceType.GetTypeInfo().IsGenericType)
            {
                foreach (var argumentType in sequenceType.GetGenericArguments())
                {
                    var ienum = typeof(IEnumerable<>).MakeGenericType(argumentType);
                    if (ienum.IsAssignableFrom(sequenceType))
                    {
                        return ienum;
                    }
                }
            }
            var interfaceTypes = sequenceType.GetInterfaces();
            if (interfaceTypes != null && interfaceTypes.Length > 0)
            {
                foreach (var interfaceType in interfaceTypes)
                {
                    var ienum = FindIEnumerable(interfaceType);
                    if (ienum != null)
                    {
                        return ienum;
                    }
                }
            }
            if (sequenceType.GetTypeInfo().BaseType != null && sequenceType.GetTypeInfo().BaseType != typeof(object))
            {
                return FindIEnumerable(sequenceType.GetTypeInfo().BaseType);
            }
            return null;
        }

        public static Type GetElementType(Type sequenceType)
        {
            var ienum = FindIEnumerable(sequenceType);
            if (ienum == null)
            {
                return sequenceType;
            }

            return ienum.GetGenericArguments()[0];
        }
    }
}
