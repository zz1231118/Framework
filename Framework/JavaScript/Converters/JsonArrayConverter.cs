using System;
using System.Collections.Generic;

namespace Framework.JavaScript.Converters
{
    /// <inheritdoc />
    public class JsonArrayConverter<T> : IJsonConverter
    {
        /// <summary>
        /// 转换到 Json
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        public Json ConvertFrom(object value, Type conversionType)
        {
            if (value == null)
                return Json.Null;
            if (value is not IEnumerable<T> collection)
                throw new ArgumentException("JsonArrayConverter 转换异常, 传入的参数类型错误!");

            var array = new JsonArray();
            foreach (var item in collection)
            {
                var json = JsonSerializer.Serialize(item);
                array.Add(json);
            }
            return array;
        }

        /// <summary>
        /// 转换到对象
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        public object? ConvertTo(Json value, Type conversionType)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (conversionType == null)
                throw new ArgumentNullException(nameof(conversionType));
            if (value == Json.Null)
                return null;
            if (value is not JsonArray jarray)
                throw new InvalidCastException();

            var array = Array.CreateInstance(typeof(T), jarray.Count);
            for (int i = 0; i < jarray.Count; i++)
            {
                var item = JsonSerializer.Deserialize<T>(jarray[i]);
                array.SetValue(item, i);
            }

            return array;
        }
    }
}
