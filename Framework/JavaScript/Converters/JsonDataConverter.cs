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
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <returns></returns>
        public static Json TargetToJson(object obj)
        {
            if (obj == null)
                return Json.Null;

            var type = obj.GetType();
            var json = new JsonObject();
            var members = JsonUtility.GetJsonMemberAttributes(type);
            foreach (var member in members)
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
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <returns></returns>
        public static object? JsonToTarget(Json json, object obj)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (json == Json.Null)
                return null;
            if (json is not JsonObject jobj)
                throw new ArgumentException("JsonDataFormat 转换异常,欲转换的 [Json] 错误!");

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
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <returns></returns>
        public static object? JsonToTarget(Json json, Type conversionType)
        {
            if (conversionType == null)
                throw new ArgumentNullException("JsonDataFormat 转换异常,欲转换的 [Type] 不能为空!");

            var target = JsonUtility.GetUninitializedObject(conversionType);
            return JsonToTarget(json, target);
        }

        /// <summary>
        /// Json 转换到 对象
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <returns></returns>
        public static TObject? JsonToTarget<TObject>(Json json)
            where TObject : class, new()
        {
            var target = JsonUtility.GetUninitializedObject(typeof(TObject));
            return (TObject?)JsonToTarget(json, target);
        }

        /// <summary>
        /// 序列化指定对象
        /// </summary>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        public Json ConvertFrom(object value, Type conversionType)
        {
            return TargetToJson(value);
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        public object? ConvertTo(Json json, Type conversionType)
        {
            return JsonToTarget(json, conversionType);
        }
    }

    /// <inheritdoc />
    public class JsonDataConverter : IJsonConverter
    {
        /// <inheritdoc />
        public Json ConvertFrom(object value, Type conversionType)
        {
            if (value == null)
                return Json.Null;

            conversionType = value.GetType();
            var json = new JsonObject();
            var members = JsonUtility.GetJsonMemberAttributes(conversionType);
            foreach (var member in members)
            {
                if (member.CanRead)
                {
                    var item = member.GetValue(value);
                    json[member.Name] = item;
                }
            }

            return json;
        }

        /// <inheritdoc />
        public object? ConvertTo(Json value, Type conversionType)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (conversionType == null)
                throw new ArgumentNullException(nameof(conversionType));
            if (value == Json.Null)
                return null;
            if (value is not JsonObject json)
                throw new ArgumentException("JsonDataFormat 转换异常,欲转换的 [Json] 错误!");

            var obj = JsonUtility.GetUninitializedObject(conversionType);
            var members = JsonUtility.GetJsonMemberAttributes(conversionType);
            foreach (var member in members)
            {
                if (member.CanWrite && json.TryGetValue(member.Name, out Json jvalue))
                {
                    member.SetValue(obj, jvalue);
                }
            }

            return obj;
        }
    }
}