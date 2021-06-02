using System;

namespace Framework.JavaScript
{
    /// <summary>
    /// JsonValue 扩展
    /// </summary>
    public static class JsonExtension
    {
        /// <summary>
        /// 把枚举转换成 JsonValue
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static JsonValue ToJsonValue(this Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return (JsonValue)Json.ConvertFrom(value.ToValue());
        }

        /// <summary>
        /// 把 JsonValue 转换成指定 枚举 类型
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static object ToEnum(this JsonValue json, Type type)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (json.Value == null)
                throw new ArgumentException("json.value is null");

            return EnumExtension.ToEnum(type, json.Value);
        }

        /// <summary>
        /// 把 JsonValue 转换成指定 枚举 类型
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static object ToEnum<T>(this JsonValue json)
            where T : struct
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (json.Value == null)
                throw new ArgumentException("json.value is null");

            return (T)EnumExtension.ToEnum(typeof(T), json.Value);
        }
    }
}