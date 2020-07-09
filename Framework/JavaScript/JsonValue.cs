using System;
using Framework.Linq;

namespace Framework.JavaScript
{
    /// <summary>
    /// JsonValue
    /// </summary>
    [Serializable]
    public sealed class JsonValue : Json, IEquatable<JsonValue>, IConvertible
    {
        private const string NullString = "null";
        private const string TrueString = "true";
        private const string FalseString = "false";
        private const string QuotationMarkString = "\"";
        private const string GuidFormat = "N";
        private const string TimeSpanFormat = @"dd\.hh\:mm\:ss\.fff";
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        private readonly object value;

        internal JsonValue(object value)
        {
            this.value = value;
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(bool value)
        {
            this.value = value;
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(byte value)
        {
            this.value = value;
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(sbyte value)
        {
            this.value = value;
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(short value)
        {
            this.value = value;
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(ushort value)
        {
            this.value = value;
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(int value)
        {
            this.value = value;
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(uint value)
        {
            this.value = value;
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(long value)
        {
            this.value = value;
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(ulong value)
        {
            this.value = value;
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(float value)
        {
            this.value = value;
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(double value)
        {
            this.value = value;
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(decimal value)
        {
            this.value = value;
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(char value)
        {
            this.value = value.ToString();
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(string value)
        {
            this.value = value;
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(Guid value)
        {
            this.value = value.ToString(GuidFormat);
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(TimeSpan value)
        {
            this.value = value.ToString(TimeSpanFormat);
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(DateTime value)
        {
            this.value = value.ToString(DateTimeFormat);
        }

        /// <summary>
        /// JsonValue 构造函数
        /// </summary>
        public JsonValue(Enum value)
        {
            if (value == null)
            {
                this.value = null;
                return;
            }

            var enumType = value.GetType();
            var underlyingType = Enum.GetUnderlyingType(enumType);
            this.value = Convert.ChangeType(value, underlyingType);
        }

        /// <summary>
        /// JsonValue 值
        /// </summary>
        public object Value => value;

        public static bool operator ==(JsonValue lhs, JsonValue rhs)
        {
            if (lhs is object && rhs is object) return lhs.value == rhs.value;
            else return ReferenceEquals(lhs, rhs);
        }

        public static bool operator !=(JsonValue lhs, JsonValue rhs)
        {
            if (lhs is object && rhs is object) return lhs.value != rhs.value;
            else return !ReferenceEquals(lhs, rhs);
        }

        /// <inheritdoc />
        public static implicit operator JsonValue(bool value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(byte value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(sbyte value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(short value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(ushort value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(int value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(uint value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(long value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(ulong value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(float value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(double value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(decimal value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(char value)
        {
            return new JsonValue(value.ToString());
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(string value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(Guid value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(TimeSpan value)
        {
            return new JsonValue(value);
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(DateTime value)
        {
            return new JsonValue(value.ToString());
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(Enum value)
        {
            return new JsonValue(value);
        }

        /// <inheritdoc />
        public static implicit operator JsonValue(bool? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }
        /// <inheritdoc />>
        public static implicit operator JsonValue(byte? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(sbyte? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(short? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(ushort? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(int? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(uint? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(long? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(ulong? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(float? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(double? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(decimal? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(char? value)
        {
            return value.HasValue ? new JsonValue(value.Value.ToString()) : (JsonValue)Null;
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(Guid? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(TimeSpan? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }
        /// <inheritdoc />
        public static implicit operator JsonValue(DateTime? value)
        {
            return value.HasValue ? new JsonValue(value.Value) : (JsonValue)Null;
        }

        /// <inheritdoc />
        public static implicit operator bool(JsonValue value)
        {
            return (bool)value.value;
        }
        /// <inheritdoc />
        public static implicit operator byte(JsonValue value)
        {
            return Convert.ToByte(value.value);
        }
        /// <inheritdoc />
        public static implicit operator sbyte(JsonValue value)
        {
            return Convert.ToSByte(value.value);
        }
        /// <inheritdoc />
        public static implicit operator short(JsonValue value)
        {
            return Convert.ToInt16(value.value);
        }
        /// <inheritdoc />
        public static implicit operator ushort(JsonValue value)
        {
            return Convert.ToUInt16(value.value);
        }
        /// <inheritdoc />
        public static implicit operator int(JsonValue value)
        {
            return Convert.ToInt32(value.value);
        }
        /// <inheritdoc />
        public static implicit operator uint(JsonValue value)
        {
            return Convert.ToUInt32(value.value);
        }
        /// <inheritdoc />
        public static implicit operator long(JsonValue value)
        {
            return Convert.ToInt64(value.value);
        }
        /// <inheritdoc />
        public static implicit operator ulong(JsonValue value)
        {
            return Convert.ToUInt64(value.value);
        }
        /// <inheritdoc />
        public static implicit operator float(JsonValue value)
        {
            return Convert.ToSingle(value.value);
        }
        /// <inheritdoc />
        public static implicit operator double(JsonValue value)
        {
            return Convert.ToDouble(value.value);
        }
        /// <inheritdoc />
        public static implicit operator decimal(JsonValue value)
        {
            return Convert.ToDecimal(value.value);
        }
        /// <inheritdoc />
        public static implicit operator char(JsonValue value)
        {
            return (char)value.value;
        }
        /// <inheritdoc />
        public static implicit operator string(JsonValue value)
        {
            return (string)value.value;
        }
        /// <inheritdoc />
        public static implicit operator Guid(JsonValue value)
        {
            return Guid.Parse((string)value.value);
        }
        /// <inheritdoc />
        public static implicit operator TimeSpan(JsonValue value)
        {
            return TimeSpan.Parse((string)value.value);
        }
        /// <inheritdoc />
        public static implicit operator DateTime(JsonValue value)
        {
            return DateTime.Parse((string)value.value);
        }

        /// <inheritdoc />
        public static implicit operator bool?(JsonValue value)
        {
            return value.value == null ? null : (bool?)(bool)value.value;
        }
        /// <inheritdoc />
        public static implicit operator byte?(JsonValue value)
        {
            return value.value == null ? null : (byte?)Convert.ToByte(value.value);
        }
        /// <inheritdoc />
        public static implicit operator sbyte?(JsonValue value)
        {
            return value.value == null ? null : (sbyte?)Convert.ToSByte(value.value);
        }
        /// <inheritdoc />
        public static implicit operator short?(JsonValue value)
        {
            return value.value == null ? null : (short?)Convert.ToInt16(value.value);
        }
        /// <inheritdoc />
        public static implicit operator ushort?(JsonValue value)
        {
            return value.value == null ? null : (ushort?)Convert.ToUInt16(value.value);
        }
        /// <inheritdoc />
        public static implicit operator int?(JsonValue value)
        {
            return value.value == null ? null : (int?)Convert.ToInt32(value.value);
        }
        /// <inheritdoc />
        public static implicit operator uint?(JsonValue value)
        {
            return value.value == null ? null : (uint?)Convert.ToUInt32(value.value);
        }
        /// <inheritdoc />
        public static implicit operator long?(JsonValue value)
        {
            return value.value == null ? null : (long?)Convert.ToInt64(value.value);
        }
        /// <inheritdoc />
        public static implicit operator ulong?(JsonValue value)
        {
            return value.value == null ? null : (ulong?)Convert.ToUInt64(value.value);
        }
        /// <inheritdoc />
        public static implicit operator float?(JsonValue value)
        {
            return value.value == null ? null : (float?)Convert.ToSingle(value.value);
        }
        /// <inheritdoc />
        public static implicit operator double?(JsonValue value)
        {
            return value.value == null ? null : (double?)Convert.ToDouble(value.value);
        }
        /// <inheritdoc />
        public static implicit operator decimal?(JsonValue value)
        {
            return value.value == null ? null : (decimal?)Convert.ToDecimal(value.value);
        }
        /// <inheritdoc />
        public static implicit operator char?(JsonValue value)
        {
            return value.value == null ? null : (char?)(char)value.value;
        }
        /// <inheritdoc />
        public static implicit operator Guid?(JsonValue value)
        {
            return value.value == null ? null : (Guid?)Guid.Parse((string)value.value);
        }
        /// <inheritdoc />
        public static implicit operator TimeSpan?(JsonValue value)
        {
            return value.value == null ? null : (TimeSpan?)TimeSpan.Parse((string)value.value);
        }
        /// <inheritdoc />
        public static implicit operator DateTime?(JsonValue value)
        {
            return value.value == null ? null : (DateTime?)DateTime.Parse((string)value.value);
        }

        /// <summary>
        /// 深度克隆
        /// </summary>
        public override object Clone()
        {
            return new JsonValue(value);
        }

        /// <summary>
        /// 返回表示当前对象的 字节集
        /// </summary>
        public override byte[] ToBinary()
        {
            var codeType = JsonUtility.GetTypeCode(value);
            switch (codeType)
            {
                case JsonTypeCode.Null:
                    return new byte[] { (byte)codeType };
                case JsonTypeCode.Boolean:
                    return new byte[] { (byte)codeType, (byte)(((bool)value) ? 1 : 0) };
                case JsonTypeCode.Byte:
                    return new byte[] { (byte)codeType, (byte)value };
                case JsonTypeCode.SByte:
                    return new byte[] { (byte)codeType, (byte)(sbyte)value };
                case JsonTypeCode.Char:
                    return JsonUtility.BuildArray(codeType, (short)(char)value);
                case JsonTypeCode.Int16:
                    return JsonUtility.BuildArray(codeType, (short)value);
                case JsonTypeCode.UInt16:
                    return JsonUtility.BuildArray(codeType, (short)(ushort)value);
                case JsonTypeCode.Int32:
                    return JsonUtility.BuildArray(codeType, (int)value);
                case JsonTypeCode.UInt32:
                    return JsonUtility.BuildArray(codeType, (int)(uint)value);
                case JsonTypeCode.Int64:
                    return JsonUtility.BuildArray(codeType, (long)value);
                case JsonTypeCode.UInt64:
                    return JsonUtility.BuildArray(codeType, (long)(ulong)value);
                case JsonTypeCode.Single:
                    return JsonUtility.BuildArray(codeType, (float)value);
                case JsonTypeCode.Double:
                    return JsonUtility.BuildArray(codeType, (double)value);
                case JsonTypeCode.Decimal:
                    return JsonUtility.BuildArray(codeType, (double)(decimal)value);
                case JsonTypeCode.String:
                    return JsonUtility.BuildArray(codeType, (string)value);
                default:
                    throw new InvalidOperationException(string.Format("Unknown {0}:{1}", nameof(JsonTypeCode), codeType));
            }
        }

        /// <summary>
        /// 返回表示当前对象的 字符串
        /// </summary>
        public override string ToString()
        {
            switch (value)
            {
                case null: return NullString;
                case bool other: return other ? TrueString : FalseString;
                case string other: return QuotationMarkString + JsonUtility.Transferred(other) + QuotationMarkString;
                default: return value.ToString();
            }
        }

        /// <summary>
        /// 用作特定类型的哈希函数。
        /// </summary>
        public override int GetHashCode()
        {
            return value != null ? value.GetHashCode() : 0;
        }

        /// <summary>
        /// 确定指定的 System.Object 是否等于当前的 System.Object。
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is JsonValue other && Equals(other);
        }

        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return (bool)value;
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(value, provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(value, provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(value, provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(value, provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(value, provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(value, provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(value, provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(value, provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(value, provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(value, provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(value, provider);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(value, provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return DateTime.Parse((string)value);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return Convert.ToString(value, provider);
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == null)
                throw new ArgumentNullException(nameof(conversionType));

            if (conversionType == typeof(Guid))
                return Guid.Parse((string)value);
            if (conversionType == typeof(TimeSpan))
                return TimeSpan.Parse((string)value);
            if (conversionType == typeof(Json))
                return this;
            if (conversionType == typeof(JsonValue))
                return this;
            if (typeof(Json).IsAssignableFrom(conversionType) && value == null)
                return null;

            if (conversionType.IsEnum)
            {
                if (this.value == null)
                    throw new InvalidCastException();
                if (!JsonUtility.IsNumber(this.value))
                    throw new InvalidCastException();

                var underlyingType = Enum.GetUnderlyingType(conversionType);
                var value = Convert.ChangeType(this.value, underlyingType);
                return Enum.ToObject(conversionType, value);
            }
            if (conversionType.IsNullable())
            {
                if (this.value == null)
                    return Activator.CreateInstance(conversionType);
                if (!JsonUtility.IsNumber(this.value))
                    throw new InvalidCastException();

                var underlyingType = Nullable.GetUnderlyingType(conversionType);
                var value = ((IConvertible)this).ToType(underlyingType, provider);
                return Activator.CreateInstance(conversionType, new object[] { value });
            }

            var typeCode = Type.GetTypeCode(conversionType);
            switch (typeCode)
            {
                case TypeCode.Object:
                    return this;
                case TypeCode.Boolean:
                    return (bool)value;
                case TypeCode.Char:
                    return Convert.ToChar(value, provider);
                case TypeCode.SByte:
                    return Convert.ToSByte(value, provider);
                case TypeCode.Byte:
                    return Convert.ToByte(value, provider);
                case TypeCode.Int16:
                    return Convert.ToInt16(value, provider);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(value, provider);
                case TypeCode.Int32:
                    return Convert.ToInt32(value, provider);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(value, provider);
                case TypeCode.Int64:
                    return Convert.ToInt64(value, provider);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(value, provider);
                case TypeCode.Single:
                    return Convert.ToSingle(value, provider);
                case TypeCode.Double:
                    return Convert.ToDouble(value, provider);
                case TypeCode.Decimal:
                    return Convert.ToDecimal(value, provider);
                case TypeCode.DateTime:
                    return DateTime.Parse((string)value);
                case TypeCode.String:
                    return Convert.ToString(value, provider);
                default:
                    throw new InvalidCastException(nameof(conversionType));
            }
        }
    }
}