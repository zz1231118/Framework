using System;
using System.Collections.Generic;

namespace Framework.JavaScript.Converters
{
    public class JsonArrayConverter<T> : IJsonConverter
    {
        /// <summary>
        /// 转换到 Json
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <exception cref="Framework.Jsons.JsonerializerException"></exception>
        public Json ConvertFrom(object value, Type conversionType)
        {
            if (value == null)
                return Json.Null;
            if (!(value is IEnumerable<T>))
                throw new ArgumentException("JsonArrayConverter 转换异常, 传入的参数类型错误!");

            var collection = value as IEnumerable<T>;
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
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonerializerException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        public object ConvertTo(Json value, Type conversionType)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (conversionType == null)
                throw new ArgumentNullException(nameof(conversionType));
            if (value == Json.Null)
                return null;
            if (!(value is JsonArray jarray))
                throw new InvalidCastException();

            var array = Array.CreateInstance(typeof(T), jarray.Count) as Array;
            for (int i = 0; i < jarray.Count; i++)
            {
                var item = JsonSerializer.Deserialize<T>(jarray[i]);
                array.SetValue(item, i);
            }

            return array;
        }
    }
}
