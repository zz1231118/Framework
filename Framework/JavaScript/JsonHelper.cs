using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Framework.JavaScript
{
    /// <summary>
    /// Json Helper
    /// </summary>
    public static class JsonHelper
    {
        internal static JsonTypeCode GetTypeCode(object value)
        {
            if (value == null)
                return JsonTypeCode.Null;

            var valueType = value.GetType();
            var codeType = Type.GetTypeCode(valueType);
            switch (codeType)
            {
                case TypeCode.Boolean:
                    return JsonTypeCode.Boolean;
                case TypeCode.Char:
                    return JsonTypeCode.Char;
                case TypeCode.SByte:
                    return JsonTypeCode.SByte;
                case TypeCode.Byte:
                    return JsonTypeCode.Byte;
                case TypeCode.Int16:
                    return JsonTypeCode.Int16;
                case TypeCode.UInt16:
                    return JsonTypeCode.UInt16;
                case TypeCode.Int32:
                    return JsonTypeCode.Int32;
                case TypeCode.UInt32:
                    return JsonTypeCode.UInt32;
                case TypeCode.Int64:
                    return JsonTypeCode.Int64;
                case TypeCode.UInt64:
                    return JsonTypeCode.UInt64;
                case TypeCode.Single:
                    return JsonTypeCode.Single;
                case TypeCode.Double:
                    return JsonTypeCode.Double;
                case TypeCode.Decimal:
                    return JsonTypeCode.Decimal;
                case TypeCode.String:
                    return JsonTypeCode.String;
            }

            throw new ArgumentException();
        }
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

        internal static byte[] BuildArray(JsonTypeCode code, short value)
        {
            var array = new byte[3];
            array[0] = (byte)code;
            BitHelper.SetBytes(array, 1, value);
            return array;
        }
        internal static byte[] BuildArray(JsonTypeCode code, int value)
        {
            var array = new byte[5];
            array[0] = (byte)code;
            BitHelper.SetBytes(array, 1, value);
            return array;
        }
        internal static byte[] BuildArray(JsonTypeCode code, long value)
        {
            var array = new byte[9];
            array[0] = (byte)code;
            BitHelper.SetBytes(array, 1, value);
            return array;
        }
        internal static byte[] BuildArray(JsonTypeCode code, float value)
        {
            var array = new byte[5];
            array[0] = (byte)code;
            BitHelper.SetBytes(array, 1, value);
            return array;
        }
        internal static byte[] BuildArray(JsonTypeCode code, double value)
        {
            var array = new byte[9];
            array[0] = (byte)code;
            BitHelper.SetBytes(array, 1, value);
            return array;
        }
        internal static byte[] BuildArray(JsonTypeCode code, string value)
        {
            var array = Jsonetting.Encoding.GetBytes(value);
            var bytes = new byte[1 + sizeof(int) + array.Length];
            bytes[0] = (byte)code;
            BitHelper.SetBytes(bytes, 1, array.Length);
            Buffer.BlockCopy(array, 0, bytes, 1 + sizeof(int), array.Length);
            return bytes;
        }
        internal static byte[] BuildArray(JsonTypeCode code, DateTimeOffset value)
        {
            var bytes = new byte[1 + sizeof(long) + sizeof(long)];
            bytes[0] = (byte)code;
            BitHelper.SetBytes(bytes, 1, value.Date.Ticks);
            BitHelper.SetBytes(bytes, 1 + sizeof(long), value.Offset.Ticks);
            return bytes;
        }
        internal static byte[] BuildArray(JsonTypeCode code, Guid value)
        {
            var array = value.ToByteArray();
            var bytes = new byte[1 + array.Length];
            bytes[0] = (byte)code;
            Buffer.BlockCopy(array, 0, bytes, 1, array.Length);
            return bytes;
        }

        /// <summary>
        /// 获取指定类型的所有 JsonMemberAttribute 标记成员
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static JsonMemberAttribute[] GetJsonMemberAttributes(Type type, bool inherit = true)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var result = new List<JsonMemberAttribute>();
            foreach (var property in type.GetProperties(bindingAttr))
            {
                var att = JsonHelper.GetJsonMemberAttribute(property, inherit);
                if (att != null)
                    result.Add(att);
            }
            return result.ToArray();
        }
        /// <summary>
        /// 获取指定类型的所有 JsonMemberAttribute 标记成员
        /// </summary>
        /// <param name="type"></param>
        /// <param name="bindingAttr"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static JsonMemberAttribute[] GetJsonMemberAttributes(Type type, BindingFlags bindingAttr, bool inherit = true)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var result = new List<JsonMemberAttribute>();
            foreach (var property in type.GetProperties(bindingAttr))
            {
                var att = JsonHelper.GetJsonMemberAttribute(property, inherit);
                if (att != null)
                    result.Add(att);
            }
            return result.ToArray();
        }
        public static JsonMemberAttribute GetJsonMemberAttribute(PropertyInfo property, bool inherit = true)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var att = property.GetCustomAttribute<JsonMemberAttribute>(inherit);
            if (att != null)
            {
                att.PropertyInfo = property;
            }

            return att;
        }
        /// <summary>
        /// 字符串 转义 处理
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        internal static string Transferred(string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            /* \’ 单引号
             * \” 双引号
             * \\ 反斜杠
             * \0 空
             * \a 警告（产生峰鸣）
             * \b 退格
             * \f 换页
             * \n 换行
             * \r 回车
             * \t 水平制表符
             * \v 垂直制表符
             */

            char ch;
            var sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                ch = str[i];
                switch (ch)
                {
                    case '\'':
                        sb.Append('\\').Append('\'');
                        break;
                    case '"':
                        sb.Append('\\').Append('"');
                        break;
                    case '\\':
                        sb.Append('\\').Append('\\');
                        break;
                    case '\0':
                        sb.Append('\\').Append('0');
                        break;
                    case '\a':
                        sb.Append('\\').Append('a');
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
                    case '\v':
                        sb.Append('\\').Append('v');
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }

            return sb.ToString();
        }
        /// <summary>
        /// Bson 推荐
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static bool BsonRecommend(Json json)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (json is JsonBinary)
                return true;

            var ja = json as JsonArray;
            if (ja != null)
                return ja.Any(p => BsonRecommend(p));

            var jo = json as JsonObject;
            if (jo != null)
                return jo.Values.Any(p => BsonRecommend(p));

            return false;
        }
    }
}