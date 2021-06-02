using System;
using System.Collections.Generic;

namespace Framework.JavaScript.Converters
{
    /// <inheritdoc />
    public class JsonCollectionConverter<T, TCollection> : IJsonConverter
        where TCollection : class, ICollection<T>
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
            if (value is not TCollection collection)
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

            var collection = (ICollection<T>)Activator.CreateInstance(typeof(TCollection), true);
            foreach (var json in jarray)
            {
                var item = JsonSerializer.Deserialize<T>(json);
                if (item == null) throw new JsonSerializerException("item is null.");

                collection.Add(item);
            }
            return collection;
        }
    }

    /// <inheritdoc />
    public class JsonCollectionConverter<T> : IJsonConverter
    {
        /// <summary>
        /// 转换到 Json
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        public Json ConvertFrom(object value, Type type)
        {
            if (value == null)
                return Json.Null;
            if (value is not ICollection<T> collection)
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
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        public object? ConvertTo(Json value, Type type)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!typeof(ICollection<T>).IsAssignableFrom(type))
                throw new ArgumentException("JsonListFormat 转换异常,传入的 [Type] 必须是实现了 ICollection<T> 的类型!");
            if (value == Json.Null)
                return null;
            if (value is not JsonArray jarray)
                throw new InvalidCastException();

            var collection = (ICollection<T>)Activator.CreateInstance(type, true);
            foreach (var json in jarray)
            {
                var item = JsonSerializer.Deserialize<T>(json);
                if (item == null) throw new JsonSerializerException("item is null.");

                collection.Add(item);
            }
            return collection;
        }
    }
}
