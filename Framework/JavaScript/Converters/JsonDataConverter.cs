using System;
using System.Linq;

namespace Framework.JavaScript.Converters
{
    /// <summary>
    /// 提供通用 类型(包含类) 到 Json 之间的转换
    /// </summary>
    public class JsonDataConverter<T> : IJsonConverter
    {
        /// <summary>
        /// 对象到 Json
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <returns></returns>
        public static Json TargetToJson(object obj)
        {
            if (obj == null)
                return Json.Null;

            var type = obj.GetType();
            var json = new JsonObject();
            var members = JsonUtility.GetJsonMemberAttributes(type);
            foreach (var member in members.OrderBy(p => p.ShowIndex))
            {
                if (member.CanRead)
                {
                    var value = member.GetValue(obj);
                    json[member.Name] = value;
                }
            }

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
                throw new ArgumentException("JsonDataFormat 转换异常,欲转换的 [Json] 错误!");

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
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <returns></returns>
        public static object JsonToTarget(Json json, Type conversionType)
        {
            if (conversionType == null)
                throw new ArgumentNullException("JsonDataFormat 转换异常,欲转换的 [Type] 不能为空!");

            var target = JsonUtility.GetUninitializedObject(conversionType);
            return JsonToTarget(json, target);
        }

        /// <summary>
        /// Json 转换到 对象
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <returns></returns>
        public static TObject JsonToTarget<TObject>(Json json)
            where TObject : class, new()
        {
            var target = JsonUtility.GetUninitializedObject(typeof(TObject));
            var result = JsonToTarget(json, target);
            return result == null ? null : result as TObject;
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

    public class JsonDataConverter : IJsonConverter
    {
        public object ConvertTo(Json value, Type conversionType)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (conversionType == null)
                throw new ArgumentNullException(nameof(conversionType));
            if (value == Json.Null)
                return null;
            if (!(value is JsonObject json))
                throw new ArgumentException("JsonDataFormat 转换异常,欲转换的 [Json] 错误!");

            Json jvalue;
            var obj = JsonUtility.GetUninitializedObject(conversionType);
            var members = JsonUtility.GetJsonMemberAttributes(conversionType);
            foreach (var member in members)
            {
                if (member.CanWrite && json.TryGetValue(member.Name, out jvalue))
                {
                    member.SetValue(obj, jvalue);
                }
            }

            return obj;
        }

        public Json ConvertFrom(object value, Type conversionType)
        {
            if (value == null)
                return Json.Null;

            conversionType = value.GetType();
            var json = new JsonObject();
            var members = JsonUtility.GetJsonMemberAttributes(conversionType);
            foreach (var member in members.OrderBy(p => p.ShowIndex))
            {
                if (member.CanRead)
                {
                    var item = member.GetValue(value);
                    json[member.Name] = item;
                }
            }

            return json;
        }
    }
}