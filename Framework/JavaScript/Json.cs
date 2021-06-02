using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Framework.JavaScript.Utility;
using Framework.Linq;

namespace Framework.JavaScript
{
    /// <summary>
    /// Json
    /// </summary>
    [Serializable]
    public abstract class Json : ICloneable, IEquatable<Json>, IEquatable<JsonValue>
    {
        /// <summary>
        /// Json.Null
        /// </summary>
        public static readonly Json Null = new JsonValue((object?)null);

        /// <inheritdoc />
        public static bool operator ==(Json? left, Json? right)
        {
            if (left is JsonValue vleft && right is JsonValue vright) return vleft.Value == vright.Value;
            else return ReferenceEquals(left, right);
        }

        /// <inheritdoc />
        public static bool operator !=(Json? left, Json? right)
        {
            if (left is JsonValue vleft && right is JsonValue vright) return vleft.Value != vright.Value;
            else return !ReferenceEquals(left, right);
        }

        /// <inheritdoc />
        public static bool operator ==(Json? left, JsonValue? right)
        {
            if (left is JsonValue vleft && right is object) return vleft.Value == right.Value;
            else return ReferenceEquals(left, right);
        }

        /// <inheritdoc />
        public static bool operator !=(Json? left, JsonValue? right)
        {
            if (left is JsonValue vleft && right is object) return vleft.Value != right.Value;
            else return !ReferenceEquals(left, right);
        }

        /// <inheritdoc />
        public static bool operator ==(JsonValue? left, Json? right)
        {
            if (left is object && right is JsonValue vright) return left.Value == vright.Value;
            else return ReferenceEquals(left, right);
        }

        /// <inheritdoc />
        public static bool operator !=(JsonValue? left, Json? right)
        {
            if (left is object && right is JsonValue vright) return left.Value != vright.Value;
            else return !ReferenceEquals(left, right);
        }

        //implicit 隐式转换
        //explicit 显式转换

        #region 正向转换

        /*
         * 在这里加隐式转换只为能获得更简便的使用
         * 但却有点恶心
         */
        #region JsonValue

        /// <inheritdoc />
        public static implicit operator Json(bool value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(byte value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(sbyte value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(short value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(ushort value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(int value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(uint value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(long value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(ulong value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(float value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(double value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(decimal value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(char value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(string value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(Guid value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(TimeSpan value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(DateTime value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator Json(Enum value)
        {
            return new JsonValue(value);
        }

        /// <inheritdoc />
        public static implicit operator Json(bool? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(byte? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(sbyte? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(short? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(ushort? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(int? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(uint? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(long? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(ulong? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(float? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(double? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(decimal? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(char? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(Guid? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(TimeSpan? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        /// <inheritdoc />
        public static implicit operator Json(DateTime? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : Json.Null;
        }
        #endregion
        #endregion

        #region 反向转换

        #region JsonValue

        /// <inheritdoc />
        public static implicit operator bool(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator byte(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator sbyte(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator short(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator ushort(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator int(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator uint(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator long(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator ulong(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator float(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator double(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator decimal(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator char(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator string?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator Guid(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator TimeSpan(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator DateTime(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator bool?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator byte?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator sbyte?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator short?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator ushort?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator int?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator uint?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator long?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator ulong?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator float?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator double?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator decimal?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator char?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator Guid?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator TimeSpan?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator DateTime?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }

        /// <inheritdoc />
        public static implicit operator byte[]?(Json value)
        {
            return value is JsonValue other ? other : throw new InvalidCastException();
        }
        #endregion
        #endregion

        /// <summary>
        /// 返回指定类型是否可与 Json 互为转换
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsDefined(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsNullable())
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                return IsDefined(underlyingType);
            }
            return type.IsPrimitive || type.IsEnum ||
                type == typeof(decimal) ||
                type == typeof(string) ||
                type == typeof(Guid) ||
                type == typeof(TimeSpan) ||
                type == typeof(DateTime) ||
                type == typeof(byte[]) ||
                typeof(Json).IsAssignableFrom(type);
        }

        /// <summary>
        /// 返回指定对象是否可与 Json 互为转换
        /// </summary>
        public static bool IsDefined(object? value)
        {
            if (value == null)
                return true;

            return IsDefined(value.GetType());
        }

        /// <summary>
        /// 把未知类型转换成 Json
        /// </summary>
        /// <exception cref="JsonException"></exception>
        public static Json ConvertFrom(object? value)
        {
            switch (value)
            {
                case null: return Json.Null;
                case Json other: return other;
                case bool other: return new JsonValue(other);
                case byte other: return new JsonValue(other);
                case sbyte other: return new JsonValue(other);
                case short other: return new JsonValue(other);
                case ushort other: return new JsonValue(other);
                case int other: return new JsonValue(other);
                case uint other: return new JsonValue(other);
                case long other: return new JsonValue(other);
                case ulong other: return new JsonValue(other);
                case float other: return new JsonValue(other);
                case double other: return new JsonValue(other);
                case decimal other: return new JsonValue(other);
                case char other: return new JsonValue(other);
                case string other: return new JsonValue(other);
                case Guid other: return new JsonValue(other);
                case TimeSpan other: return new JsonValue(other);
                case DateTime other: return new JsonValue(other);
                case Enum other: return new JsonValue(other);
                case byte[] other: return new JsonValue(other);
            }

            #region JsonArray

            if (value is IList list)
            {
                var json = new JsonArray();
                foreach (var item in list)
                {
                    json.Add(ConvertFrom(item));
                }
                return json;
            }
            #endregion

            #region JsonObject

            if (value is IDictionary dictionary)
            {
                var json = new JsonObject();
                foreach (DictionaryEntry entity in dictionary)
                {
                    json.Add(entity.Key.ToString(), ConvertFrom(entity.Value));
                }
                return json;
            }
            #endregion

            throw new InvalidCastException("转换失败,未找到匹配的类型");
        }

        /// <summary>
        /// 把未知类型转换成 Json
        /// </summary>
        /// <exception cref="JsonException"></exception>
        public static T ConvertFrom<T>(object? value)
            where T : Json
        {
            return (T)ConvertFrom(value);
        }

        /// <summary>
        /// 把 Json 转换成 指定类型的对象
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        public static object? ConvertTo(Json json, Type type)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (type == typeof(Json))
                return json;

            switch (json)
            {
                case JsonValue other: return ((IConvertible)other).ToType(type, CultureInfo.InvariantCulture);
                case JsonArray other:
                    if (type == typeof(JsonArray))
                    {
                        return other;
                    }
                    else if (type.IsSubclassOf(typeof(IList)))
                    {
                        var genericType = MetaType.GetListItemType(type);
                        if (genericType != null)
                        {
                            var list = (IList)Activator.CreateInstance(type, true);
                            foreach (var jsonItem in other)
                            {
                                list.Add(ConvertTo(jsonItem, genericType));
                            }

                            return list;
                        }
                    }
                    break;
                case JsonObject other:
                    if (type == typeof(JsonObject))
                        return other;
                    else if (type.IsSubclassOf(typeof(IDictionary)))
                    {
                        var genericArray = MetaType.GetDictionaryItemType(type);
                        if (genericArray.Length == 2)
                        {
                            var keyType = genericArray[0];
                            if (keyType == typeof(string))
                            {
                                var valueType = genericArray[1];
                                var dictionary = (IDictionary)Activator.CreateInstance(type, true);
                                foreach (var key in other.Keys)
                                {
                                    var value = ConvertTo(other[key], valueType);
                                    dictionary.Add(key, value);
                                }

                                return dictionary;
                            }
                        }
                    }
                    break;
            }

            throw new InvalidCastException("json cast failed. unknown type: " + type.FullName);
        }

        /// <summary>
        /// 把 Json 转换成 指定类型的对象
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        public static T? ConvertTo<T>(Json json)
        {
            return (T)ConvertTo(json, typeof(T));
        }

        /// <summary>
        /// 把 Json 格式的文本转换成 Json
        /// </summary>
        /// <returns>返回解析的 Json</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        public static Json Parse(string text)
        {
            return JsonParser.Parse(text);
        }

        /// <summary>
        /// 把 Json 格式的文本转换成 Json
        /// </summary>
        /// <returns>返回解析的 Json</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        public static T Parse<T>(string text)
            where T : Json
        {
            return (T)JsonParser.Parse(text);
        }

        /// <summary>
        /// 将指定样式和区域性特定格式的 Json 的字符串表示形式转换为它的等效 Json。一个指示转换是否成功的返回值。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static bool TryParse(string text, out Json? json)
        {
            try
            {
                json = JsonParser.Parse(text);
                return true;
            }
            catch (Exception)
            {
                json = null;
                return false;
            }
        }

        /// <summary>
        /// 将指定样式和区域性特定格式的 Json 的字符串表示形式转换为它的等效 Json。一个指示转换是否成功的返回值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static bool TryParse<T>(string text, out T? json)
            where T : Json
        {
            if (TryParse(text, out Json? value) && value is T rval)
            {
                json = rval;
                return true;
            }

            json = null;
            return false;
        }

        /// <summary>
        /// 克隆
        /// </summary>
        public abstract object Clone();

        /// <inheritdoc />
        public bool Equals(Json other)
        {
            return this is JsonValue vleft && other is JsonValue vright ? vleft.Value == vright.Value : ReferenceEquals(this, other);
        }

        /// <inheritdoc />
        public bool Equals(JsonValue other)
        {
            return this is JsonValue vleft && other is object ? vleft.Value == other.Value : ReferenceEquals(this, other);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Json other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}