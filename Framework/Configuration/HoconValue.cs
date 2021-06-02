using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Framework.Configuration
{
    public class HoconValue : IMightBeAHoconObject
    {
        private readonly List<IHoconElement> values = new List<IHoconElement>();

        public bool IsEmpty => values.Count == 0 || (values[0] is HoconObject first && first.Items.Count == 0);
        public List<IHoconElement> Values => values;

        private static double ParsePositiveValue(string v)
        {
            var value = double.Parse(v, NumberFormatInfo.InvariantInfo);
            if (value < 0)
                throw new FormatException("Expected a positive value instead of " + value);
            return value;
        }
        private string QuoteIfNeeded(string text)
        {
            if (text == null) return "";
            if (text.ToCharArray().Intersect(" \t".ToCharArray()).Any())
            {
                return "\"" + text + "\"";
            }
            return text;
        }

        public Config AtKey(string key)
        {
            var o = new HoconObject();
            o.GetOrCreateKey(key);
            o.Items[key] = this;
            var r = new HoconValue();
            r.values.Add(o);
            return new Config(new HoconRoot(r));
        }
        public HoconObject GetObject()
        {
            IHoconElement raw = values.FirstOrDefault();
            if (raw is HoconObject o) return o;
            if (raw is IMightBeAHoconObject sub && sub.IsObject()) return sub.GetObject();
            return null;
        }
        public bool IsObject()
        {
            return GetObject() != null;
        }
        public bool IsArray()
        {
            return GetArray() != null;
        }
        public void AppendValue(IHoconElement value)
        {
            values.Add(value);
        }
        public void Clear()
        {
            values.Clear();
        }
        public void NewValue(IHoconElement value)
        {
            values.Clear();
            values.Add(value);
        }
        public bool IsString()
        {
            return values.Any() && values.All(v => v.IsString());
        }
        private string? ConcatString()
        {
            string concat = string.Join("", values.Select(p => p.GetString())).Trim();
            if (concat == "null")
                return null;

            return concat;
        }
        public HoconValue GetChildObject(string key)
        {
            return GetObject().GetKey(key);
        }
        public bool GetBoolean()
        {
            string v = GetString();
            switch (v)
            {
                case "on":
                    return true;
                case "off":
                    return false;
                case "true":
                    return true;
                case "false":
                    return false;
                default:
                    throw new NotSupportedException("Unknown boolean format: " + v);
            }
        }
        public string GetString()
        {
            if (IsString())
            {
                return ConcatString();
            }
            return null;
        }
        public byte GetByte()
        {
            return byte.Parse(GetString(), NumberFormatInfo.InvariantInfo);
        }
        public sbyte GetSByte()
        {
            return sbyte.Parse(GetString(), NumberFormatInfo.InvariantInfo);
        }
        public short GetInt16()
        {
            return short.Parse(GetString(), NumberFormatInfo.InvariantInfo);
        }
        public ushort GetUInt16()
        {
            return ushort.Parse(GetString(), NumberFormatInfo.InvariantInfo);
        }
        public int GetInt32()
        {
            return int.Parse(GetString(), NumberFormatInfo.InvariantInfo);
        }
        public uint GetUInt32()
        {
            return uint.Parse(GetString(), NumberFormatInfo.InvariantInfo);
        }
        public long GetInt64()
        {
            return long.Parse(GetString(), NumberFormatInfo.InvariantInfo);
        }
        public ulong GetUInt64()
        {
            return ulong.Parse(GetString(), NumberFormatInfo.InvariantInfo);
        }
        public float GetSingle()
        {
            return float.Parse(GetString(), NumberFormatInfo.InvariantInfo);
        }
        public double GetDouble()
        {
            return double.Parse(GetString(), NumberFormatInfo.InvariantInfo);
        }
        public decimal GetDecimal()
        {
            return decimal.Parse(GetString(), NumberFormatInfo.InvariantInfo);
        }
        public bool[] GetBooleanArray()
        {
            return GetArray().Select(v => v.GetBoolean()).ToArray();
        }
        public byte[] GetByteArray()
        {
            return GetArray().Select(v => v.GetByte()).ToArray();
        }
        public sbyte[] GetSByteArray()
        {
            return GetArray().Select(v => v.GetSByte()).ToArray();
        }
        public short[] GetInt16Array()
        {
            return GetArray().Select(v => v.GetInt16()).ToArray();
        }
        public ushort[] GetUInt16Array()
        {
            return GetArray().Select(v => v.GetUInt16()).ToArray();
        }
        public int[] GetInt32Array()
        {
            return GetArray().Select(v => v.GetInt32()).ToArray();
        }
        public uint[] GetUInt32Array()
        {
            return GetArray().Select(v => v.GetUInt32()).ToArray();
        }
        public long[] GetInt64Array()
        {
            return GetArray().Select(v => v.GetInt64()).ToArray();
        }
        public ulong[] GetUInt64Array()
        {
            return GetArray().Select(v => v.GetUInt64()).ToArray();
        }
        public float[] GetSingleArray()
        {
            return GetArray().Select(v => v.GetSingle()).ToArray();
        }
        public double[] GetDoubleArray()
        {
            return GetArray().Select(v => v.GetDouble()).ToArray();
        }
        public decimal[] GetDecimalArray()
        {
            return GetArray().Select(v => v.GetDecimal()).ToArray();
        }
        public string[] GetStringArray()
        {
            return GetArray().Select(v => v.GetString()).ToArray();
        }
        public HoconValue[] GetArray()
        {
            return values.Where(p => p.IsArray()).SelectMany(p => p.GetArray()).ToArray();
        }
        public TimeSpan GetTimeSpan(bool allowInfinite = true)
        {
            string res = GetString();
            if (res.EndsWith("ms"))
            {
                var v = res.Substring(0, res.Length - 2);
                return TimeSpan.FromMilliseconds(ParsePositiveValue(v));
            }
            if (res.EndsWith("s"))
            {
                var v = res.Substring(0, res.Length - 1);
                return TimeSpan.FromSeconds(ParsePositiveValue(v));
            }
            if (res.EndsWith("m"))
            {
                var v = res.Substring(0, res.Length - 1);
                return TimeSpan.FromMinutes(ParsePositiveValue(v));
            }
            if (res.EndsWith("h"))
            {
                var v = res.Substring(0, res.Length - 1);
                return TimeSpan.FromHours(ParsePositiveValue(v));
            }
            if (res.EndsWith("d"))
            {
                var v = res.Substring(0, res.Length - 1);
                return TimeSpan.FromDays(ParsePositiveValue(v));
            }
            if (allowInfinite && res.Equals("infinite", StringComparison.OrdinalIgnoreCase))  //Not in Hocon spec
            {
                return TimeSpan.FromMilliseconds(Timeout.Infinite);
                //return Timeout.InfiniteTimeSpan;
            }

            return TimeSpan.FromMilliseconds(ParsePositiveValue(res));
        }
        public DateTime GetDateTime()
        {
            return DateTime.Parse(GetString(), CultureInfo.InvariantCulture);
        }
        public Guid GetGuid()
        {
            return Guid.Parse(GetString());
        }
        public Enum GetEnum(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentException(nameof(enumType));

            var underlyingType = Enum.GetUnderlyingType(enumType);
            switch (Type.GetTypeCode(underlyingType))
            {
                case TypeCode.Byte:
                    return (Enum)Enum.ToObject(enumType, byte.Parse(GetString(), NumberFormatInfo.InvariantInfo));
                case TypeCode.SByte:
                    return (Enum)Enum.ToObject(enumType, sbyte.Parse(GetString(), NumberFormatInfo.InvariantInfo));
                case TypeCode.Int16:
                    return (Enum)Enum.ToObject(enumType, short.Parse(GetString(), NumberFormatInfo.InvariantInfo));
                case TypeCode.UInt16:
                    return (Enum)Enum.ToObject(enumType, ushort.Parse(GetString(), NumberFormatInfo.InvariantInfo));
                case TypeCode.Int32:
                    return (Enum)Enum.ToObject(enumType, int.Parse(GetString(), NumberFormatInfo.InvariantInfo));
                case TypeCode.UInt32:
                    return (Enum)Enum.ToObject(enumType, uint.Parse(GetString(), NumberFormatInfo.InvariantInfo));
                case TypeCode.Int64:
                    return (Enum)Enum.ToObject(enumType, long.Parse(GetString(), NumberFormatInfo.InvariantInfo));
                case TypeCode.UInt64:
                    return (Enum)Enum.ToObject(enumType, ulong.Parse(GetString(), NumberFormatInfo.InvariantInfo));
                default:
                    throw new InvalidOperationException("unknown type: " + enumType);
            }
        }
        public T GetEnum<T>()
            where T : Enum
        {
            return (T)GetEnum(typeof(T));
        }
        public long? GetByteSize()
        {
            var res = GetString();
            if (res.EndsWith("b"))
            {
                var v = res.Substring(0, res.Length - 1);
                return long.Parse(v);
            }

            return long.Parse(res);
        }
        public override string ToString()
        {
            return ToString(0);
        }
        public virtual string ToString(int indent)
        {
            if (IsString())
            {
                string text = QuoteIfNeeded(GetString());
                return text;
            }
            if (IsObject())
            {
                var i = new string(' ', indent * 2);
                return string.Format("{{\r\n{1}{0}}}", i, GetObject().ToString(indent + 1));
            }
            if (IsArray())
            {
                return string.Format("[{0}]", string.Join(",", GetArray().Select(e => e.ToString(indent + 1))));
            }
            return "<<unknown value>>";
        }
    }
}
