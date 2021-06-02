using System;
using System.Collections;
using System.Linq;
using Framework.JavaScript.Converters;
using Framework.JavaScript.Utility;

namespace Framework.JavaScript
{
    /// <summary>
    /// 支持 Json 序列化
    /// </summary>
    public static class JsonSerializer
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        /// <return>序列化的 Json</return>
        public static Json Serialize(object? obj)
        {
            if (obj == null)
                return Json.Null;

            var type = obj.GetType();
            if (Json.IsDefined(type))
                return Json.ConvertFrom(obj);
            if (obj is IList list)
                return ListSerialize(list);
            if (obj is IDictionary dictionary)
                return DictionarySerialize(dictionary);
            if (type.IsClass || type.IsValueType)
            {
                if (obj is IJsonDynamicObject jdobj)
                {
                    return DynamicDataSerialize(jdobj);
                }
                else
                {
                    return DataSerialize(obj);
                }
            }

            throw new JsonSerializerException("Json 序列化异常,未知类型. Type=[" + type.FullName + "]");
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        public static T Serialize<T>(object? obj)
            where T : Json
        {
            return (T)Serialize(obj);
        }

        /// <summary>
        /// 序列化 IList
        /// </summary>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        private static Json ListSerialize(IList iList)
        {
            var array = new JsonArray();
            foreach (var value in iList)
            {
                array.Add(Serialize(value));
            }

            return array;
        }

        /// <summary>
        /// 序列化 IDictionary
        /// </summary>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        private static Json DictionarySerialize(IDictionary kv)
        {
            var array = new JsonArray();
            foreach (var key in kv.Keys)
            {
                var jobj = new JsonObject();
                jobj["Key"] = Serialize(key);
                jobj["Value"] = Serialize(kv[key]);
                array.Add(jobj);
            }

            return array;
        }

        /// <summary>
        /// 序列化 类
        /// </summary>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        private static JsonObject DataSerialize(object obj)
        {
            var type = obj.GetType();
            var members = JsonUtility.GetJsonMemberAttributes(type);
            var json = new JsonObject();
            foreach (var member in members)
            {
                if (member.CanRead)
                {
                    try
                    {
                        var jval = member.GetValue(obj);
                        json[member.Name] = jval;
                    }
                    catch (Exception ex)
                    {
                        throw new JsonSerializerException("Json 序列化异常 Type.Name=[" + type.Name +
                            "] Member.Name=[" + member.Name + "] Exception.Message=[" + ex.Message + "] Exception.StackTrace=[" +
                            ex.StackTrace + "]", ex);
                    }
                }
            }

            return json;
        }

        /// <summary>
        /// 序列化 动态类
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        private static Json DynamicDataSerialize(IJsonDynamicObject obj)
        {
            var json = DataSerialize(obj);
            json[nameof(IJsonDynamicObject.ClassName)] = obj.ClassName;
            return json;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        public static object? Deserialize(Json json, object obj)
        {
            if (json is null)
                throw new ArgumentNullException(nameof(json));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (json is JsonValue jval && jval.Value == null)
                return null;

            var type = obj.GetType();
            if (Json.IsDefined(type))
            {
                return Json.ConvertTo(json, type);
            }
            if (obj is IList list)
            {
                if (json is not JsonArray array)
                    throw new JsonSerializerException("Json 序列化异常,参数错误!");

                return CollectionDeserialize(array, list);
            }
            if (type.IsClass || type.IsValueType)
            {
                if (json is not JsonObject jobj)
                    throw new JsonSerializerException("Json 序列化异常,参数错误!");

                return ClassDeserialize(jobj, obj);
            }

            throw new JsonSerializerException("Json 序列化异常,未知类型. Type=[" + type.FullName + "]");
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        public static T? Deserialize<T>(Json json, object obj)
        {
            return (T)Deserialize(json, obj);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        public static object? Deserialize(Json json, Type type)
        {
            if (json is null)
                throw new ArgumentNullException(nameof(json));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (json is JsonValue jval && jval.Value == null)
                return null;

            if (Json.IsDefined(type))
            {
                return Json.ConvertTo(json, type);
            }
            if (typeof(IList).IsAssignableFrom(type))
            {
                if (json is not JsonArray array)
                {
                    throw new JsonSerializerException($"{nameof(Deserialize)} to {type.FullName} fail. json format error.");
                }
                var list = type.IsArray
                    ? (IList)Array.CreateInstance(type.GetElementType(), array.Count)
                    : (IList)JsonUtility.GetUninitializedObject(type);

                return CollectionDeserialize(array, list);
            }
            if (type.IsClass || type.IsAbstract || type.IsInterface || type.IsValueType)
            {
                if (json is not JsonObject jobj)
                    throw new JsonSerializerException("Json 反序列化异常,参数不正确!");

                if (typeof(IJsonDynamicObject).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface)
                {
                    return DynamicDataDeserialize(jobj, type);
                }
                else
                {
                    var obj = JsonUtility.GetUninitializedObject(type);
                    return ClassDeserialize(jobj, obj);
                }
            }

            throw new JsonSerializerException("Json 反序列化异常,未知类型. Type=[" + type.FullName + "]");
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        public static T? Deserialize<T>(Json json)
        {
            return (T)Deserialize(json, typeof(T));
        }

        /// <summary>
        /// 反序列化 IList
        /// </summary>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        private static IList CollectionDeserialize(JsonArray array, IList list)
        {
            var type = list.GetType();
            if (type.IsArray)
            {
                var eleType = type.GetElementType();
                var count = Math.Min(array.Count, list.Count);
                for (int i = 0; i < count; i++)
                {
                    list[i] = Deserialize(array[i], eleType);
                }
            }
            else
            {
                var eleType = MetaType.GetListItemType(type);
                foreach (var jval in array)
                {
                    list.Add(Deserialize(jval, eleType));
                }
            }
            return list;
        }

        /// <summary>
        /// 反序列化 类
        /// </summary>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        private static object ClassDeserialize(JsonObject json, object obj)
        {
            var type = obj.GetType();
            var members = JsonUtility.GetJsonMemberAttributes(type);
            foreach (var jval in json)
            {
                var key = jval.Key;
                var member = members.FirstOrDefault(p => p.Name.Equals(key));
                if (member != null && member.CanWrite)
                {
                    try
                    {
                        var jsonValue = jval.Value;
                        member.SetValue(obj, jsonValue);
                    }
                    catch (Exception ex)
                    {
                        throw new JsonSerializerException("Json 反序列化异常 Type.Name=[" + type.Name +
                            "] Member.Name=[" + key + "] Exception.Message=[" + ex.Message +
                            "] Exception.StackTrace=[" + ex.StackTrace + "]", ex);
                    }
                }
            }

            return obj;
        }

        /// <summary>
        /// 反序列化 动态类
        /// </summary>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <exception cref="JsonSerializerException"></exception>
        private static object DynamicDataDeserialize(JsonObject json, Type? type)
        {
            if (!json.TryGetValue(nameof(IJsonDynamicObject.ClassName), out Json value))
                throw new JsonSerializerException("Json 反序列化异常,欲反向转换的数据错误 " + nameof(IJsonDynamicObject.ClassName) + " 丢失");

            var name = (string?)value;
            if (name == null)
            {
                throw new JsonSerializerException($"{nameof(Deserialize)} {nameof(IJsonDynamicObject.ClassName)} is null.");
            }
            type = JsonType.GetTypeByName(name, type);
            if (type == null)
            {
                throw new JsonSerializerException("Json 反序列化异常,转换的数据错误 未知的 Type=[" + name + "] 类型");
            }

            var obj = JsonUtility.GetUninitializedObject(type);
            return ClassDeserialize(json, obj);
        }
    }
}