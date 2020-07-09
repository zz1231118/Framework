using System;
using System.Linq;

namespace Framework.JavaScript.Converters
{
    /// <summary>
    /// JsonDynamicData
    /// </summary>
    public class JsonDynamicDataConverter : IJsonConverter
    {
        /// <summary>
        /// 对象 转换到 Json
        /// </summary>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <returns></returns>
        public static Json TargetToJson(object obj)
        {
            if (obj == null)
                return Json.Null;

            var type = obj.GetType();
            var members = JsonUtility.GetJsonMemberAttributes(type);
            var json = new JsonObject();
            foreach (var member in members.OrderBy(p => p.ShowIndex))
            {
                if (member.CanRead)
                {
                    var jval = member.GetValue(obj);
                    json[member.Name] = jval;
                }
            }

            json[nameof(IJsonDynamicObject.ClassName)] = JsonType.GetNameByType(type);
            return json;
        }

        /// <summary>
        /// Json 转换到 对象
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <returns></returns>
        public static object JsonToTarget(Json json, object obj)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (json == Json.Null)
                return null;
            if (!(json is JsonObject))
                throw new ArgumentException("JsonDynamicDataFormat 转换异常,[Json] 类型错误!");

            var jobj = json as JsonObject;
            var type = obj.GetType();
            var members = JsonUtility.GetJsonMemberAttributes(type);
            foreach (var member in members)
            {
                if (member.CanWrite && jobj.ContainsKey(member.Name))
                {
                    var value = jobj[member.Name];
                    member.SetValue(obj, value);
                }
            }

            return obj;
        }

        /// <summary>
        /// Json 转换到 对象
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <returns></returns>
        public static object JsonToTarget(Json json, Type conversionType)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (json == Json.Null)
                return null;
            if (!(json is JsonObject))
                throw new ArgumentException("JsonDynamicDataFormat 转换异常, [Json] 类型错误!");

            var jobj = json as JsonObject;
            if (!jobj.ContainsKey("ClassName"))
                throw new JsonFormatException("JsonDynamicDataFormat 转换异常,[ClassName] 丢失!");

            string typeName = jobj["ClassName"];
            Type itemType = JsonType.GetTypeByName(typeName, conversionType);
            if (itemType == null)
                throw new JsonFormatException("JsonDynamicDataFormat 转换异常,未找到类型 [Type]=[" + typeName + "]!");

            var target = JsonUtility.GetUninitializedObject(itemType);
            var members = JsonUtility.GetJsonMemberAttributes(itemType);
            foreach (var member in members)
            {
                if (member.CanWrite && jobj.ContainsKey(member.Name))
                {
                    var value = jobj[member.Name];
                    member.SetValue(target, value);
                }
            }

            return target;
        }

        /// <summary>
        /// 序列化指定对象
        /// </summary>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonerializerException"></exception>
        public Json ConvertFrom(object value, Type conversionType)
        {
            return TargetToJson(value);
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <exception cref="Framework.Jsons.JsonerializerException"></exception>
        public object ConvertTo(Json json, Type conversionType)
        {
            return JsonToTarget(json, conversionType);
        }
    }
    /// <summary>
    /// JsonDynamicData
    /// </summary>
    public class JsonDynamicDataConverter<T> : JsonDynamicDataConverter
        where T : class
    { }
}