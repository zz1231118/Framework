using System;
using System.Collections;
using System.Linq;
using System.Reflection;
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
        /// <exception cref="Framework.JavaScript.JsonFormatException"></exception>
        /// <exception cref="Framework.JavaScript.JsonerializerException"></exception>
        /// <exception cref="Framework.JavaScript.JsonException"></exception>
        /// <return>序列化的 Json</return>
        public static Json Serialize(object obj, bool checkAttribute = true)
        {
            if (obj == null)
                return Json.Null;

            var type = obj.GetType();
            if (Json.IsDefined(type))
                return Json.ConvertTo(obj);
            if (typeof(IList).IsAssignableFrom(type))
                return ListSerialize(obj as IList);
            if (typeof(IDictionary).IsAssignableFrom(type))
                return DictionarySerialize(obj as IDictionary);
            if (type.IsClass || type.IsValueType)
            {
                if (!checkAttribute)
                    return NoCharacteristicSerialize(obj);

                if (typeof(IJsonDynamicObject).IsAssignableFrom(type))
                {
                    return DynamicDataSerialize(obj as IJsonDynamicObject);
                }
                else
                {
                    return DataSerialize(obj);
                }
            }

            throw new JsonerializerException("Json 序列化异常,未知类型. Type=[" + type.FullName + "]");
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <exception cref="Framework.JavaScript.JsonFormatException"></exception>
        /// <exception cref="Framework.JavaScript.JsonerializerException"></exception>
        /// <exception cref="Framework.JavaScript.JsonException"></exception>
        public static T Serialize<T>(object obj, bool checkAttribute = true)
            where T : Json
        {
            return (T)Serialize(obj, checkAttribute);
        }
        /// <summary>
        /// 序列化 IList
        /// </summary>
        /// <exception cref="Framework.JavaScript.JsonFormatException"></exception>
        /// <exception cref="Framework.JavaScript.JsonerializerException"></exception>
        /// <exception cref="Framework.JavaScript.JsonException"></exception>
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
        /// <exception cref="Framework.JavaScript.JsonFormatException"></exception>
        /// <exception cref="Framework.JavaScript.JsonerializerException"></exception>
        /// <exception cref="Framework.JavaScript.JsonException"></exception>
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
        /// <exception cref="Framework.JavaScript.JsonFormatException"></exception>
        /// <exception cref="Framework.JavaScript.JsonerializerException"></exception>
        private static JsonObject DataSerialize(object obj)
        {
            var type = obj.GetType();
            var members = JsonUtility.GetJsonMemberAttributes(type);
            var json = new JsonObject();
            foreach (var member in members.OrderBy(p => p.ShowIndex))
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
                        throw new JsonerializerException("Json 序列化异常 Type.Name=[" + type.Name +
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
        /// <exception cref="Framework.JavaScript.JsonFormatException"></exception>
        /// <exception cref="Framework.JavaScript.JsonerializerException"></exception>
        private static Json DynamicDataSerialize(IJsonDynamicObject obj)
        {
            var json = DataSerialize(obj);
            json[nameof(IJsonDynamicObject.ClassName)] = obj.ClassName;
            return json;
        }
        /// <summary>
        /// 无特性 序列化
        /// </summary>
        private static Json NoCharacteristicSerialize(object obj)
        {
            var type = obj.GetType();
            var json = new JsonObject();
            var bindAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var property in type.GetProperties(bindAttr))
            {
                if (property.CanRead)
                {
                    var value = property.GetValue(obj, null);
                    var jsval = Serialize(value, false);
                    json[property.Name] = jsval;
                }
            }

            return json;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="Framework.JavaScript.JsonFormatException"></exception>
        /// <exception cref="Framework.JavaScript.JsonerializerException"></exception>
        /// <exception cref="Framework.JavaScript.JsonException"></exception>
        public static object Deserialize(Json json, object obj, bool checkAttribute = true)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (json is JsonValue jval && jval.Value == null)
                return null;

            var type = obj.GetType();
            if (Json.IsDefined(type))
            {
                return Json.ChangeType(json, type);
            }
            if (obj is IList)
            {
                if (!(json is JsonArray))
                    throw new JsonerializerException("Json 序列化异常,参数错误!");

                return CollectionDeserialize(json as JsonArray, obj as IList);
            }
            if (type.IsClass || type.IsValueType)
            {
                if (!(json is JsonObject))
                    throw new JsonerializerException("Json 序列化异常,参数错误!");

                return ClassDeserialize(json as JsonObject, obj);
            }

            throw new JsonerializerException("Json 序列化异常,未知类型. Type=[" + type.FullName + "]");
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="Framework.JavaScript.JsonFormatException"></exception>
        /// <exception cref="Framework.JavaScript.JsonerializerException"></exception>
        /// <exception cref="Framework.JavaScript.JsonException"></exception>
        public static T Deserialize<T>(Json json, object obj, bool checkAttribute = true)
        {
            var desObj = Deserialize(json, obj);
            return desObj == null ? default(T) : (T)desObj;
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="Framework.JavaScript.JsonFormatException"></exception>
        /// <exception cref="Framework.JavaScript.JsonerializerException"></exception>
        /// <exception cref="Framework.JavaScript.JsonException"></exception>
        public static object Deserialize(Json json, Type type, bool checkAttribute = true)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (json is JsonValue jval && jval.Value == null)
                return null;

            if (Json.IsDefined(type))
            {
                return Json.ChangeType(json, type);
            }
            if (typeof(IList).IsAssignableFrom(type))
            {
                var array = json as JsonArray;
                object obj = type.IsArray
                    ? Array.CreateInstance(type.GetElementType(), array.Count)
                    : JsonUtility.GetUninitializedObject(type);

                return CollectionDeserialize(array, obj as IList);
            }
            if (type.IsClass || type.IsAbstract || type.IsInterface || type.IsValueType)
            {
                if (!(json is JsonObject))
                    throw new JsonerializerException("Json 反序列化异常,参数不正确!");
                if (!checkAttribute)
                    return NoCharacteristicDeserialize(json as JsonObject, type);

                if (typeof(IJsonDynamicObject).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface)
                {
                    return DynamicDataDeserialize(json as JsonObject, type);
                }
                else
                {
                    var obj = JsonUtility.GetUninitializedObject(type);
                    return ClassDeserialize(json as JsonObject, obj);
                }
            }

            throw new JsonerializerException("Json 反序列化异常,未知类型. Type=[" + type.FullName + "]");
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="Framework.JavaScript.JsonFormatException"></exception>
        /// <exception cref="Framework.JavaScript.JsonerializerException"></exception>
        /// <exception cref="Framework.JavaScript.JsonException"></exception>
        public static T Deserialize<T>(Json json, bool checkAttribute = true)
        {
            var obj = Deserialize(json, typeof(T), checkAttribute);
            return obj == null ? default(T) : (T)obj;
        }
        /// <summary>
        /// 反序列化 IList
        /// </summary>
        /// <exception cref="Framework.JavaScript.JsonFormatException"></exception>
        /// <exception cref="Framework.JavaScript.JsonerializerException"></exception>
        /// <exception cref="Framework.JavaScript.JsonException"></exception>
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
        /// <exception cref="Framework.JavaScript.JsonFormatException"></exception>
        /// <exception cref="Framework.JavaScript.JsonerializerException"></exception>
        /// <exception cref="Framework.JavaScript.JsonException"></exception>
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
                        throw new JsonerializerException("Json 反序列化异常 Type.Name=[" + type.Name +
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
        /// <exception cref="Framework.JavaScript.JsonFormatException"></exception>
        /// <exception cref="Framework.JavaScript.JsonerializerException"></exception>
        /// <exception cref="Framework.JavaScript.JsonException"></exception>
        private static object DynamicDataDeserialize(JsonObject json, Type type)
        {
            if (!json.ContainsKey(nameof(IJsonDynamicObject.ClassName)))
                throw new JsonerializerException("Json 反序列化异常,欲反向转换的数据错误 " + nameof(IJsonDynamicObject.ClassName) + " 丢失");

            string typeName = (string)json[nameof(IJsonDynamicObject.ClassName)];
            var targetType = JsonType.GetTypeByName(typeName, type);
            if (targetType == null)
                throw new JsonerializerException("Json 反序列化异常,转换的数据错误 未知的 Type=[" + typeName + "] 类型");

            var obj = JsonUtility.GetUninitializedObject(targetType);
            return ClassDeserialize(json, obj);
        }
        /// <summary>
        /// 无特性 反序列化
        /// </summary>
        private static object NoCharacteristicDeserialize(JsonObject json, Type type)
        {
            if (typeof(IJsonDynamicObject).IsAssignableFrom(type) || type.IsInterface)
            {
                if (!json.ContainsKey(nameof(IJsonDynamicObject.ClassName)))
                    throw new JsonerializerException(string.Format("Json 反序列化异常,{0}.{1} 丢失", nameof(IJsonDynamicObject), nameof(IJsonDynamicObject.ClassName)));

                var typeName = (string)json[nameof(IJsonDynamicObject.ClassName)];
                type = JsonType.GetTypeByName(typeName, type);
            }

            var obj = JsonUtility.GetUninitializedObject(type);
            var bindAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var properties = type.GetProperties(bindAttr).ToList();
            foreach (var name in json.Keys)
            {
                var property = properties.FirstOrDefault(p => p.Name.Equals(name));
                if (property != null && property.CanWrite)
                {
                    var jval = json[name];
                    var value = Deserialize(jval, property.PropertyType, false);
                    property.SetValue(obj, value, null);
                }
            }

            return obj;
        }
    }
}