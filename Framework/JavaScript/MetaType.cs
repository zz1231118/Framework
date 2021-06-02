using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Framework.JavaScript.Utility
{
    internal class MetaType
    {
        public static Type GetListItemType(Type listType)
        {
            if (listType == null)
                throw new ArgumentNullException(nameof(listType));
            if (listType.IsArray || !typeof(IEnumerable).IsAssignableFrom(listType))
                return null;

            var candidates = new List<Type>();
            var bindingAttr = BindingFlags.Instance | BindingFlags.Public;
            foreach (var method in listType.GetMethods(bindingAttr))
            {
                if (method.Name != "Add")
                    continue;

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length > 0)
                {
                    Type paramType = parameters[0].ParameterType;
                    if (parameters.Length == 1 && !candidates.Contains(paramType))
                        candidates.Add(paramType);
                }
            }

            string name = listType.Name;
            bool isQueueStack = name != null && (name.IndexOf("Queue") >= 0 || name.IndexOf("Stack") >= 0);
            if (!isQueueStack)
            {
                TestEnumerableListPatterns(candidates, listType);
                foreach (Type iType in listType.GetInterfaces())
                    TestEnumerableListPatterns(candidates, iType);
            }

            bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (PropertyInfo property in listType.GetProperties(bindingAttr))
            {
                if (property.Name != "Item" || candidates.Contains(property.PropertyType))
                    continue;
                ParameterInfo[] args = property.GetIndexParameters();
                if (args.Length != 1 || args[0].ParameterType != typeof(int))
                    continue;
                candidates.Add(property.PropertyType);
            }
            switch (candidates.Count)
            {
                case 0:
                    return null;
                case 1:
                    return candidates[0];
                case 2:
                    if (CheckDictionaryAccessors(candidates[0], candidates[1]))
                        return candidates[0];
                    if (CheckDictionaryAccessors(candidates[1], candidates[0]))
                        return candidates[1];
                    break;
            }

            return null;
        }

        public static Type[] GetDictionaryItemType(Type kvType)
        {
            if (kvType == null)
                throw new ArgumentNullException(nameof(kvType));
            if (!typeof(IEnumerable).IsAssignableFrom(kvType))
                return null;

            var candidates = new List<Type>();
            var bindingAttr = BindingFlags.Instance | BindingFlags.Public;
            foreach (var method in kvType.GetMethods(bindingAttr))
            {
                if (method.Name != "Add")
                    continue;

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 2)
                {
                    Type paramKeyType = parameters[0].ParameterType;
                    Type paramValueType = parameters[1].ParameterType;
                    if (!candidates.Contains(paramKeyType))
                        candidates.Add(paramKeyType);
                    if (!candidates.Contains(paramValueType))
                        candidates.Add(paramValueType);
                }
            }

            foreach (var iType in kvType.GetInterfaces())
                TestEnumerableDictionaryPatterns(candidates, iType);

            bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (PropertyInfo property in kvType.GetProperties(bindingAttr))
            {
                if (property.Name != "Item" || candidates.Contains(property.PropertyType))
                    continue;
                ParameterInfo[] args = property.GetIndexParameters();
                if (args.Length != 1 || args[0].ParameterType != typeof(int))
                    continue;
                candidates.Add(property.PropertyType);
            }
            switch (candidates.Count)
            {
                case 0:
                case 1:
                    return null;
                case 2:
                    return candidates.ToArray();
                case 3:
                    if (CheckDictionaryAccessors(candidates[2], candidates[0], 0) &&
                        CheckDictionaryAccessors(candidates[2], candidates[1], 1))
                        return candidates.Take(2).ToArray();
                    break;
            }

            return null;
        }

        private static void TestEnumerableListPatterns(List<Type> candidates, Type iType)
        {
            if (iType.IsGenericType)
            {
                Type typeDef = iType.GetGenericTypeDefinition();
                if (typeDef == typeof(IEnumerable<>)
                    || typeDef == typeof(ICollection<>)
                    || typeDef.FullName == "System.Collections.Concurrent.IProducerConsumerCollection`1")
                {
                    Type[] iTypeArgs = iType.GetGenericArguments();
                    if (!candidates.Contains(iTypeArgs[0]))
                        candidates.Add(iTypeArgs[0]);
                }
            }
        }

        private static void TestEnumerableDictionaryPatterns(List<Type> candidates, Type iType)
        {
            if (iType.IsGenericType)
            {
                Type typeDef = iType.GetGenericTypeDefinition();
                if (typeDef == typeof(IDictionary<,>))
                {
                    Type[] iTypeArgs = iType.GetGenericArguments();
                    var ikType = iTypeArgs[0];
                    var ivType = iTypeArgs[1];
                    if (!candidates.Contains(ikType))
                        candidates.Add(ikType);
                    if (!candidates.Contains(ivType))
                        candidates.Add(ivType);
                }
                else if (typeDef == typeof(IEnumerable<>)
                    || typeDef == typeof(ICollection<>)
                    || typeDef.FullName == "System.Collections.Concurrent.IProducerConsumerCollection`1")
                {
                    Type[] iTypeArgs = iType.GetGenericArguments();
                    foreach (var iTypeArg in iTypeArgs)
                    {
                        if (iTypeArg.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                        {
                            var isTypeArgs = iTypeArg.GetGenericArguments();
                            var ikType = isTypeArgs[0];
                            var ivType = isTypeArgs[1];
                            if (!candidates.Contains(ikType))
                                candidates.Add(ikType);
                            if (!candidates.Contains(ivType))
                                candidates.Add(ivType);
                        }
                    }
                }
            }
        }

        private static bool CheckDictionaryAccessors(Type pair, Type value)
        {
            return pair.IsGenericType && pair.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)
                && pair.GetGenericArguments()[1] == value;
        }

        private static bool CheckDictionaryAccessors(Type pair, Type value, int genIndex)
        {
            return pair.IsGenericType && pair.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)
                && pair.GetGenericArguments()[genIndex] == value;
        }
    }
}