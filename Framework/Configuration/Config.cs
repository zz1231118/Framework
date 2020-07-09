using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Framework.Configuration.Hocon;

namespace Framework.Configuration
{
    public class Config
    {
        public static readonly Config Empty = Parse("");

        public Config()
        { }
        public Config(HoconRoot root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            if (root.Value == null)
                throw new ArgumentNullException("root.Value");

            Root = root.Value;
            Substitutions = root.Substitutions;
        }
        public Config(Config source, Config fallback)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Root = source.Root;
            Fallback = fallback;
        }

        public Config Fallback { get; private set; }
        public virtual bool IsEmpty => Root == null || Root.IsEmpty;
        public virtual HoconValue Root { get; private set; }
        public IEnumerable<HoconSubstitution> Substitutions { get; set; }

        public static Config operator +(Config config, string fallback)
        {
            Config fallbackConfig = Config.Parse(fallback);
            return config.WithFallback(fallbackConfig);
        }
        public static Config operator +(string configHocon, Config fallbackConfig)
        {
            Config config = Config.Parse(configHocon);
            return config.WithFallback(fallbackConfig);
        }

        public static Config Parse(string text)
        {
            return Parse(text, null);
        }
        public static Config Parse(string text, Func<string, HoconRoot> includeCallback)
        {
            var res = Parser.Parse(text, includeCallback);
            return new Config(res);
        }
        public static Config LoadFile(string path, Encoding encoding)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

            return Parse(File.ReadAllText(path, encoding));
        }
        public static Config LoadFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return LoadFile(path, Encoding.UTF8);
        }

        private HoconValue GetNode(string path)
        {
            var node = Root;
            if (node == null)
                throw new InvalidOperationException("Current node should not be null");

            var elements = path.Split('.');
            foreach (string element in elements)
            {
                node = node.GetChildObject(element);
                if (node == null)
                {
                    if (Fallback != null)
                        return Fallback.GetNode(path);

                    return null;
                }
            }
            return node;
        }
        protected Config Copy()
        {
            //deep clone
            return new Config
            {
                Fallback = Fallback?.Copy(),
                Root = Root,
                Substitutions = Substitutions
            };
        }
        public virtual long? GetByteSize(string path)
        {
            var value = GetNode(path);
            return value?.GetByteSize();
        }
        public virtual bool GetBoolean(string path, bool @default = false)
        {
            var value = GetNode(path);
            return value != null ? value.GetBoolean() : @default;
        }
        public virtual byte GetByte(string path, byte @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetByte() : @default;
        }
        public virtual sbyte GetSByte(string path, sbyte @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetSByte() : @default;
        }
        public virtual short GetInt16(string path, short @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetInt16() : @default;
        }
        public virtual ushort GetUInt16(string path, ushort @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetUInt16() : @default;
        }
        public virtual int GetInt32(string path, int @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetInt32() : @default;
        }
        public virtual uint GetUInt32(string path, uint @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetUInt32() : @default;
        }
        public virtual long GetInt64(string path, long @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetInt64() : @default;
        }
        public virtual ulong GetUInt64(string path, ulong @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetUInt64() : @default;
        }
        public virtual string GetString(string path, string @default = null)
        {
            var value = GetNode(path);
            return value != null ? value.GetString() : @default;
        }
        public virtual float GetSingle(string path, float @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetSingle() : @default;
        }
        public virtual double GetDouble(string path, double @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetDouble() : @default;
        }
        public virtual decimal GetDecimal(string path, decimal @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetDecimal() : @default;
        }
        public virtual TimeSpan GetTimeSpan(string path, TimeSpan? @default = null, bool allowInfinite = true)
        {
            var value = GetNode(path);
            return value != null ? value.GetTimeSpan(allowInfinite) : @default.GetValueOrDefault();
        }
        public virtual DateTime GetDateTime(string path, DateTime? @default = null)
        {
            var value = GetNode(path);
            return value != null ? value.GetDateTime() : @default.GetValueOrDefault();
        }
        public virtual Guid GetGuid(string path, Guid? @default = null)
        {
            var value = GetNode(path);
            return value != null ? value.GetGuid() : @default.GetValueOrDefault();
        }
        public virtual Enum GetEnum(Type enumType, string path, Enum @default = null)
        {
            if (enumType == null)
                throw new ArgumentException(nameof(enumType));

            var value = GetNode(path);
            return value != null ? value.GetEnum(enumType) : @default;
        }
        public virtual T GetEnum<T>(string path, T? @default = null)
            where T : struct, Enum
        {
            var value = GetNode(path);
            return value != null ? value.GetEnum<T>() : @default.GetValueOrDefault();
        }

        public virtual bool[] GetBooleanArray(string path)
        {
            var value = GetNode(path);
            return value.GetBooleanArray();
        }
        public virtual byte[] GetByteArray(string path)
        {
            var value = GetNode(path);
            return value.GetByteArray();
        }
        public virtual sbyte[] GetSByteArray(string path)
        {
            var value = GetNode(path);
            return value.GetSByteArray();
        }
        public virtual short[] GetInt16Array(string path)
        {
            var value = GetNode(path);
            return value.GetInt16Array();
        }
        public virtual ushort[] GetUInt16Array(string path)
        {
            var value = GetNode(path);
            return value.GetUInt16Array();
        }
        public virtual int[] GetInt32Array(string path)
        {
            var value = GetNode(path);
            return value.GetInt32Array();
        }
        public virtual uint[] GetUInt32Array(string path)
        {
            var value = GetNode(path);
            return value.GetUInt32Array();
        }
        public virtual long[] GetInt64Array(string path)
        {
            var value = GetNode(path);
            return value.GetInt64Array();
        }
        public virtual ulong[] GetUInt64Array(string path)
        {
            var value = GetNode(path);
            return value.GetUInt64Array();
        }
        public virtual float[] GetSingleArray(string path)
        {
            var value = GetNode(path);
            return value.GetSingleArray();
        }
        public virtual double[] GetDoubleArray(string path)
        {
            var value = GetNode(path);
            return value.GetDoubleArray();
        }
        public virtual decimal[] GetDecimalArray(string path)
        {
            var value = GetNode(path);
            return value.GetDecimalArray();
        }
        public virtual string[] GetStringArray(string path)
        {
            var value = GetNode(path);
            return value.GetStringArray();
        }
        public virtual Config WithFallback(Config fallback)
        {
            if (fallback == this)
                throw new ArgumentException("Config can not have itself as fallback", "fallback");

            Config clone = Copy();
            Config current = clone;
            while (current.Fallback != null)
            {
                current = current.Fallback;
            }

            current.Fallback = fallback;
            return clone;
        }
        public virtual Config GetConfig(string path)
        {
            HoconValue value = GetNode(path);
            if (Fallback != null)
            {
                Config f = Fallback.GetConfig(path);
                if (value == null && f == null)
                    return null;
                if (value == null)
                    return f;

                return new Config(new HoconRoot(value)).WithFallback(f);
            }

            if (value == null)
                return null;

            return new Config(new HoconRoot(value));
        }
        public HoconValue GetValue(string path)
        {
            HoconValue value = GetNode(path);
            return value;
        }
        public virtual bool HasPath(string path)
        {
            return GetNode(path) != null;
        }
        public virtual IEnumerable<KeyValuePair<string, HoconValue>> AsEnumerable()
        {
            var used = new HashSet<string>();
            Config current = this;
            while (current != null)
            {
                foreach (var kvp in current.Root.GetObject().Items)
                {
                    if (!used.Contains(kvp.Key))
                    {
                        yield return kvp;
                        used.Add(kvp.Key);
                    }
                }
                current = current.Fallback;
            }
        }
        public override string ToString()
        {
            if (Root == null)
                return string.Empty;

            return Root.ToString();
        }
    }
}