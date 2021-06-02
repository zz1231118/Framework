using System;
using System.Collections;

namespace Framework.JavaScript.Converters
{
    /// <summary>
    /// 提供 Dictionary 到 Json 的转换
    /// </summary>
    public class JsonDictionaryConverter<TKey, TValue> : IJsonConverter
    {
        /// <summary>
        /// 把指定对象转换到 Json
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        public Json ConvertFrom(object value, Type conversionType)
        {
            if (value == null)
                return Json.Null;
            if (conversionType == null)
                throw new ArgumentNullException("JsonDictionaryFormat 转换异常,参数 [Type] 不能为空");
            if (value is not IDictionary dictionary)
                throw new ArgumentException("JsonDictionaryFormat 转换失败 只能转换 IDictionary 类型");

            var array = new JsonArray();
            lock (dictionary)
            {
                foreach (var key in dictionary.Keys)
                {
                    var json = new JsonObject();
                    json["Key"] = JsonSerializer.Serialize(key);
                    json["Value"] = JsonSerializer.Serialize(dictionary[key]);
                    array.Add(json);
                }
            }
            return array;
        }

        /// <summary>
        /// 把指定 Json 转换到 对象
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        public object? ConvertTo(Json json, Type conversionType)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (conversionType == null)
                throw new ArgumentNullException(nameof(conversionType));
            if (!typeof(IDictionary).IsAssignableFrom(conversionType))
                throw new JsonFormatException("JsonDictionaryFormat 反向转换失败 Type 必须继承 IDictionary");

            if (json == Json.Null)
            {
                return null;
            }
            if (json is not JsonArray array)
            {
                throw new JsonFormatException("JsonDictionaryFormat 转换失败,传入的参数类型错误");
            }

            var dictionary = (IDictionary)JsonUtility.GetUninitializedObject(conversionType);
            foreach (JsonObject jobj in array)
            {
                var key = JsonSerializer.Deserialize(jobj["Key"], typeof(TKey));
                var val = JsonSerializer.Deserialize(jobj["Value"], typeof(TValue));
                dictionary.Add(key, val);
            }

            return dictionary;
        }
    }
}