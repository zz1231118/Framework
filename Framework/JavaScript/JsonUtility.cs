using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Framework.JavaScript
{
    /// <summary>
    /// Json Helper
    /// </summary>
    public static class JsonUtility
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> serializablePropertyTable = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private static readonly Func<Type, PropertyInfo[]> serializablePropertyFactory = key =>
        {
            var depth = 0;
            var type = key;
            var rootType = typeof(object);
            var names = new HashSet<string>();
            var properties = new List<PropertyInfo>();
            var dictionary = new Dictionary<Type, int>();
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            do
            {
                dictionary[type] = depth++;
                foreach (var property in type.GetProperties(bindingFlags))
                {
                    if (names.Add(property.Name))
                    {
                        properties.Add(property);
                    }
                }

                type = type.BaseType;
            } while (type != rootType);
            return properties.OrderByDescending(p => dictionary[p.DeclaringType]).ToArray();
        };

        internal static bool IsNumber(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var codeType = Type.GetTypeCode(value.GetType());
            switch (codeType)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsInteger(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var codeType = Type.GetTypeCode(value.GetType());
            switch (codeType)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 获取指定类型的所有 JsonMemberAttribute 标记成员
        /// </summary>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static JsonMemberAttribute[] GetJsonMemberAttributes(Type type, bool inherit = true)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var result = new List<JsonMemberAttribute>();
            foreach (var property in GetSerializableProperties(type))
            {
                var member = JsonUtility.GetJsonMemberAttribute(property, inherit);
                if (member != null)
                    result.Add(member);
            }
            return result.ToArray();
        }

        /// <summary>
        /// 获取指定类型的所有 JsonMemberAttribute 标记成员
        /// </summary>
        public static JsonMemberAttribute? GetJsonMemberAttribute(PropertyInfo property, bool inherit = true)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var member = property.GetCustomAttribute<JsonMemberAttribute>(inherit);
            if (member != null) member.PropertyInfo = property;
            return member;
        }

        /// <summary>
        /// 字符串 转义 处理
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        internal static string EscapeString(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            //除了字符 "，\ ，/ 和一些控制符（ \b，\f，\n，\r，\t）需要编码外，其他 Unicode 字符可以直接输出
            var sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                switch (ch)
                {
                    case '\"':
                        sb.Append('\\').Append('"');
                        break;
                    case '\\':
                        sb.Append('\\').Append('\\');
                        break;
                    case '/':
                        sb.Append('\\').Append('/');
                        break;
                    case '\b':
                        sb.Append('\\').Append('b');
                        break;
                    case '\f':
                        sb.Append('\\').Append('f');
                        break;
                    case '\n':
                        sb.Append('\\').Append('n');
                        break;
                    case '\r':
                        sb.Append('\\').Append('r');
                        break;
                    case '\t':
                        sb.Append('\\').Append('t');
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        public static PropertyInfo[] GetSerializableProperties(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return serializablePropertyTable.GetOrAdd(type, serializablePropertyFactory);
        }

        /// <inheritdoc />
        public static object GetUninitializedObject(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var constructor = type.GetConstructor(bindingAttr, null, Type.EmptyTypes, null);
            return constructor == null ? FormatterServices.GetUninitializedObject(type) : constructor.Invoke(null);
        }
    }
}