using System;
using System.Collections.Generic;

namespace Framework.JavaScript.Converters
{
    public class JsonCollectionConverter<T, TCollection> : IJsonConverter
        where TCollection : class, ICollection<T>
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
            if (!(value is TCollection collection))
                throw new InvalidCastException();

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

            var collection = (ICollection<T>)Activator.CreateInstance(typeof(TCollection), true);
            foreach (var json in jarray)
            {
                var item = JsonSerializer.Deserialize<T>(json);
                collection.Add(item);
            }
            return collection;
        }
    }

    public class JsonCollectionConverter<T> : IJsonConverter
    {
        /// <summary>
        /// 转换到 Json
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <exception cref="Framework.Jsons.JsonerializerException"></exception>
        public Json ConvertFrom(object value, Type type)
        {
            if (value == null)
                return Json.Null;
            if (!(value is ICollection<T> collection))
                throw new InvalidCastException();

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
        public object ConvertTo(Json value, Type type)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!typeof(ICollection<T>).IsAssignableFrom(type))
                throw new ArgumentException("JsonListFormat 转换异常,传入的 [Type] 必须是实现了 ICollection<T> 的类型!");
            if (value == Json.Null)
                return null;
            if (!(value is JsonArray jarray))
                throw new InvalidCastException();

            var collection = (ICollection<T>)Activator.CreateInstance(type, true);
            foreach (var json in jarray)
            {
                var item = JsonSerializer.Deserialize<T>(json);
                collection.Add(item);
            }
            return collection;
        }
    }
}
