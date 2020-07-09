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
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <exception cref="Framework.Jsons.JsonerializerException"></exception>
        public Json ConvertFrom(object value, Type conversionType)
        {
            if (value == null)
                return Json.Null;
            if (conversionType == null)
                throw new ArgumentNullException("JsonDictionaryFormat 转换异常,参数 [Type] 不能为空");
            if (!(value is IDictionary))
                throw new ArgumentException("JsonDictionaryFormat 转换失败 只能转换 IDictionary 类型");

            var array = new JsonArray();
            var kv = value as IDictionary;
            lock (kv)
            {
                foreach (var key in kv.Keys)
                {
                    var json = new JsonObject();
                    json["Key"] = JsonSerializer.Serialize(key);
                    json["Value"] = JsonSerializer.Serialize(kv[key]);
                    array.Add(json);
                }
            }
            return array;
        }

        /// <summary>
        /// 把指定 Json 转换到 对象
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <exception cref="Framework.Jsons.JsonerializerException"></exception>
        public object ConvertTo(Json json, Type conversionType)
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
            if (!(json is JsonArray))
            {
                throw new JsonFormatException("JsonDictionaryFormat 转换失败,传入的参数类型错误");
            }

            var kv = JsonUtility.GetUninitializedObject(conversionType) as IDictionary;
            var array = json as JsonArray;
            foreach (JsonObject jobj in array)
            {
                var key = JsonSerializer.Deserialize(jobj["Key"], typeof(TKey));
                var val = JsonSerializer.Deserialize(jobj["Value"], typeof(TValue));
                kv.Add(key, val);
            }

            return kv;
        }
    }
}