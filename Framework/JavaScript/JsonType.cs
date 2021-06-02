using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Framework.JavaScript
{
    /// <summary>
    /// JsonType
    /// </summary>
    public static class JsonType
    {
        private static readonly ConcurrentDictionary<string, Type> globalTypeCache;

        static JsonType()
        {
            var builtTypes = new List<Type>()
            {
                typeof(int), typeof(uint), typeof(short), typeof(ushort), typeof(long), typeof(ulong),
                typeof(float), typeof(double), typeof(decimal), typeof(byte), typeof(sbyte), typeof(char),
                typeof(bool), typeof(string), typeof(TimeSpan), typeof(DateTime), typeof(Guid)
            };

            globalTypeCache = new ConcurrentDictionary<string, Type>();
            foreach (var type in builtTypes)
            {
                globalTypeCache[type.Name] = type;
            }
        }

        /// <summary>
        /// 获取指定 Type名称 的 Type
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        private static Type? GetTypeByName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name == string.Empty)
                throw new ArgumentException(nameof(name));

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetTypes().FirstOrDefault(p => p.Name.Equals(name));
                if (type != null) return type;
            }
            return null;
        }

        /// <summary>
        /// 当前类型是否是泛型类型
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsGenericType(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name == string.Empty)
                throw new ArgumentException(nameof(name));

            return (name.Contains("[") && name.Contains("]"));
        }

        /// <summary>
        /// 获取泛型类型
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        private static Type? GetGenericType(string name)
        {
            if (IsGenericType(name))
            {
                using (var deco = new TypeDecomposition(new StringReader(name)))
                {
                    return deco.FatherType;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取指定 Type名称 的 Type
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static Type? GetTypeByName(string name, Type? consultType = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name == string.Empty)
                throw new ArgumentException(nameof(name));

            if (globalTypeCache.TryGetValue(name, out Type? typeResult))
            {
                return typeResult;
            }
            if (IsGenericType(name))
            {
                typeResult = GetGenericType(name);
            }
            else if (consultType == null)
            {
                typeResult = GetTypeByName(name);
            }
            else
            {
                var typePath = consultType.Namespace + "." + name;
                typeResult = consultType.Assembly.GetType(typePath, false, false);

                if (typeResult == null)
                {
                    typeResult = GetTypeByName(name);
                }
            }

            if (typeResult != null)
                globalTypeCache[name] = typeResult;

            return typeResult;
        }

        /// <summary>
        /// 获取指定 Type 的 Name
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetNameByType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var builder = new StringBuilder();
            builder.Append(type.Name);

            if (type.IsGenericType)
            {
                builder.Append("[");
                var gts = type.GetGenericArguments();

                foreach (var item in gts)
                {
                    builder.Append(GetNameByType(item) + ",");
                }

                builder.Remove(builder.Length - 1, 1);
                builder.Append("]");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Type 分解
        /// </summary>
        class TypeDecomposition : IDisposable
        {
            private Type? _fatherType;
            private List<TypeDecomposition>? _generics;

            public TypeDecomposition(TextReader reader)
            {
                Decomposition(reader);
            }

            public Type? FatherType => _fatherType;

            /// <summary>
            /// 分解
            /// </summary>
            private void Decomposition(TextReader reader)
            {
                var temp = -1;
                var builder = new StringBuilder();
                while ((temp = reader.Read()) >= 0)
                {
                    if (temp == 91)
                    {
                        _generics = new List<TypeDecomposition>();
                        var deco = new TypeDecomposition(reader);
                        _generics.Add(deco);
                    }
                    else if (temp == 41)
                    {
                        if (_generics == null)
                            break;
                        else
                            _generics.Add(new TypeDecomposition(reader));
                    }
                    else if (temp == 93)
                        break;
                    else
                        builder.Append((char)temp);
                }

                _fatherType = JsonType.GetTypeByName(builder.ToString());
                if (_generics != null && _generics.Count > 0)
                {
                    if (_fatherType != null)
                    {
                        var Types = _generics.Select(p => p._fatherType).ToArray();
                        _fatherType = _fatherType.MakeGenericType(Types);
                    }
                }
            }

            public void Dispose()
            {
                _fatherType = null;
                _generics = null;
            }
        }
    }
}