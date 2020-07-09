using System;
using System.Collections;
using System.Collections.Generic;

namespace Framework.JavaScript.Converters
{
    /// <summary>
    /// JsonListDynamicFormat
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonDynamicCollectionConverter<T> : IJsonConverter
        where T : class
    {
        /// <summary>
        /// 对象 到 Json
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <returns></returns>
        public Json ConvertFrom(object value, Type conversionType)
        {
            if (value == null)
                return Json.Null;
            if (!(value is ICollection<T>))
                throw new ArgumentException("JsonListFormat 转换异常, 传入的参数类型错误!");

            var collection = value as ICollection<T>;
            var array = new JsonArray();
            lock (collection)
            {
                foreach (var val in collection)
                {
                    array.Add(JsonDynamicDataConverter.TargetToJson(val));
                }
            }
            return array;
        }

        /// <summary>
        /// Json 到 对象
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <returns></returns>
        public object ConvertTo(Json json, Type conversionType)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (conversionType == null)
                throw new ArgumentNullException(nameof(conversionType));
            if (!typeof(IList).IsAssignableFrom(conversionType))
                throw new ArgumentException("JsonListFormat 转换异常,传入的 [Type] 必须是实现了 IList 的类型!");
            if (json == Json.Null)
                return null;
            if (!(json is JsonArray))
                throw new ArgumentException("JsonListFormat 转换异常,传入的 [Json] 类型错误!");

            var collection = JsonUtility.GetUninitializedObject(conversionType) as ICollection<T>;
            var itemType = typeof(T);
            var array = json as JsonArray;
            foreach (var jval in array)
            {
                var value = JsonDynamicDataConverter.JsonToTarget(jval, itemType) as T;
                collection.Add(value);
            }

            return collection;
        }
    }

    /// <summary>
    /// JsonListDynamicFormat
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonDynamicCollectionConverter<T, TCollection> : IJsonConverter
        where T : class
        where TCollection : class, ICollection<T>
    {
        /// <summary>
        /// 对象 到 Json
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <returns></returns>
        public Json ConvertFrom(object value, Type type)
        {
            if (value == null)
                return Json.Null;
            if (!(value is ICollection<T>))
                throw new ArgumentException("JsonListFormat 转换异常, 传入的参数类型错误!");

            var collection = value as ICollection<T>;
            var array = new JsonArray();
            lock (collection)
            {
                foreach (var item in collection)
                {
                    var json = JsonDynamicDataConverter<T>.TargetToJson(item);
                    array.Add(json);
                }
            }
            return array;
        }

        /// <summary>
        /// Json 到 对象
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <returns></returns>
        public object ConvertTo(Json json, Type type)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (json == Json.Null)
                return null;
            if (!(json is JsonArray))
                throw new ArgumentException("JsonListFormat 转换异常,传入的 [Json] 类型错误!");

            var collection = JsonUtility.GetUninitializedObject(typeof(TCollection)) as ICollection<T>;
            var itemType = typeof(T);
            var array = json as JsonArray;
            foreach (var jval in array)
            {
                var value = JsonDynamicDataConverter<T>.JsonToTarget(jval, itemType) as T;
                collection.Add(value);
            }

            return collection;
        }
    }
}