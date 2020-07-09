using System;

namespace Framework.JavaScript
{
    /// <summary>
    /// JsonValue 扩展
    /// </summary>
    public static class JsonValueExpand
    {
        /// <summary>
        /// 把枚举转换成 JsonValue
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static JsonValue ToJsonValue(this Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return Json.ConvertTo(value.ToValue()) as JsonValue;
        }
        /// <summary>
        /// 把 JsonValue 转换成指定 枚举 类型
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static object ToEnum(this JsonValue json, Type type)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return EnumExtension.ToEnum(type, json.Value);
        }
        /// <summary>
        /// 把 JsonValue 转换成指定 枚举 类型
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static object ToEnum<T>(this JsonValue json)
            where T : struct
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            return (T)EnumExtension.ToEnum(typeof(T), json.Value);
        }
    }
}