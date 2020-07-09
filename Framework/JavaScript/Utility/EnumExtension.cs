using System;

namespace Framework.JavaScript
{
    /// <summary>
    /// Enum 的扩展
    /// </summary>
    static class EnumExtension
    {
        /// <summary>
        /// Enum 到 对象
        /// </summary>
        public static object ToValue(this Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var underlyingType = Enum.GetUnderlyingType(value.GetType());
            return Convert.ChangeType(value, underlyingType);
        }
        /// <summary>
        /// 对象到 Enum
        /// </summary>
        public static object ToEnum(Type type, object value)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (type.BaseType != typeof(Enum))
                throw new ArgumentException();

            if (decimal.TryParse(value.ToString(), out _))
            {
                var underlyingType = Enum.GetUnderlyingType(type);
                value = Convert.ChangeType(value, underlyingType);
                return Enum.ToObject(type, value);
            }
            else
            {
                return Enum.Parse(type, value.ToString());
            }
        }
    }
}