using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Framework.Configuration.Hocon;

namespace Framework.Configuration
{
    /// <inheritdoc />
    public class Config
    {
        /// <inheritdoc />
        public static readonly Config Empty = Parse("");

        /// <inheritdoc />
        public Config()
        { }

        /// <inheritdoc />
        public Config(HoconRoot root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            if (root.Value == null)
                throw new ArgumentNullException("root.Value");

            Root = root.Value;
            Substitutions = root.Substitutions;
        }

        /// <inheritdoc />
        public Config(Config source, Config fallback)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Root = source.Root;
            Fallback = fallback;
        }

        /// <inheritdoc />
        public virtual HoconValue? Root { get; private set; }

        /// <inheritdoc />
        public Config? Fallback { get; private set; }

        /// <inheritdoc />
        public IEnumerable<HoconSubstitution>? Substitutions { get; set; }

        /// <inheritdoc />
        public virtual bool IsEmpty => Root == null || Root.IsEmpty;

        /// <inheritdoc />
        public static Config operator +(Config config, string fallback)
        {
            Config fallbackConfig = Config.Parse(fallback);
            return config.WithFallback(fallbackConfig);
        }

        /// <inheritdoc />
        public static Config operator +(string configHocon, Config fallbackConfig)
        {
            Config config = Config.Parse(configHocon);
            return config.WithFallback(fallbackConfig);
        }

        /// <inheritdoc />
        public static Config Parse(string text)
        {
            return Parse(text, null);
        }

        /// <inheritdoc />
        public static Config Parse(string text, Func<string, HoconRoot>? includeCallback)
        {
            var res = Parser.Parse(text, includeCallback);
            return new Config(res);
        }

        /// <inheritdoc />
        public static Config LoadFile(string path, Encoding encoding)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

            return Parse(File.ReadAllText(path, encoding));
        }

        /// <inheritdoc />
        public static Config LoadFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return LoadFile(path, Encoding.UTF8);
        }

        private HoconValue? GetNode(string path)
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual long? GetByteSize(string path)
        {
            var value = GetNode(path);
            return value?.GetByteSize();
        }

        /// <inheritdoc />
        public virtual bool GetBoolean(string path, bool @default = false)
        {
            var value = GetNode(path);
            return value != null ? value.GetBoolean() : @default;
        }

        /// <inheritdoc />
        public virtual byte GetByte(string path, byte @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetByte() : @default;
        }

        /// <inheritdoc />
        public virtual sbyte GetSByte(string path, sbyte @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetSByte() : @default;
        }

        /// <inheritdoc />
        public virtual short GetInt16(string path, short @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetInt16() : @default;
        }

        /// <inheritdoc />
        public virtual ushort GetUInt16(string path, ushort @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetUInt16() : @default;
        }

        /// <inheritdoc />
        public virtual int GetInt32(string path, int @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetInt32() : @default;
        }

        /// <inheritdoc />
        public virtual uint GetUInt32(string path, uint @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetUInt32() : @default;
        }

        /// <inheritdoc />
        public virtual long GetInt64(string path, long @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetInt64() : @default;
        }

        /// <inheritdoc />
        public virtual ulong GetUInt64(string path, ulong @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetUInt64() : @default;
        }

        /// <inheritdoc />
        public virtual string? GetString(string path, string? @default = null)
        {
            var value = GetNode(path);
            return value != null ? value.GetString() : @default;
        }

        /// <inheritdoc />
        public virtual float GetSingle(string path, float @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetSingle() : @default;
        }

        /// <inheritdoc />
        public virtual double GetDouble(string path, double @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetDouble() : @default;
        }

        /// <inheritdoc />
        public virtual decimal GetDecimal(string path, decimal @default = 0)
        {
            var value = GetNode(path);
            return value != null ? value.GetDecimal() : @default;
        }

        /// <inheritdoc />
        public virtual TimeSpan GetTimeSpan(string path, TimeSpan? @default = null, bool allowInfinite = true)
        {
            var value = GetNode(path);
            return value != null ? value.GetTimeSpan(allowInfinite) : @default.GetValueOrDefault();
        }

        /// <inheritdoc />
        public virtual DateTime GetDateTime(string path, DateTime? @default = null)
        {
            var value = GetNode(path);
            return value != null ? value.GetDateTime() : @default.GetValueOrDefault();
        }

        /// <inheritdoc />
        public virtual Guid GetGuid(string path, Guid? @default = null)
        {
            var value = GetNode(path);
            return value != null ? value.GetGuid() : @default.GetValueOrDefault();
        }

        /// <inheritdoc />
        public virtual Enum? GetEnum(Type enumType, string path, Enum? @default = null)
        {
            if (enumType == null)
                throw new ArgumentException(nameof(enumType));

            var value = GetNode(path);
            return value != null ? value.GetEnum(enumType) : @default;
        }

        /// <inheritdoc />
        public virtual T GetEnum<T>(string path, T? @default = null)
            where T : struct, Enum
        {
            var value = GetNode(path);
            return value != null ? value.GetEnum<T>() : @default.GetValueOrDefault();
        }

        /// <inheritdoc />
        public virtual bool[]? GetBooleanArray(string path)
        {
            var value = GetNode(path);
            return value?.GetBooleanArray();
        }

        /// <inheritdoc />
        public virtual byte[]? GetByteArray(string path)
        {
            var value = GetNode(path);
            return value?.GetByteArray();
        }

        /// <inheritdoc />
        public virtual sbyte[]? GetSByteArray(string path)
        {
            var value = GetNode(path);
            return value?.GetSByteArray();
        }

        /// <inheritdoc />
        public virtual short[]? GetInt16Array(string path)
        {
            var value = GetNode(path);
            return value?.GetInt16Array();
        }

        /// <inheritdoc />
        public virtual ushort[]? GetUInt16Array(string path)
        {
            var value = GetNode(path);
            return value?.GetUInt16Array();
        }

        /// <inheritdoc />
        public virtual int[]? GetInt32Array(string path)
        {
            var value = GetNode(path);
            return value?.GetInt32Array();
        }

        /// <inheritdoc />
        public virtual uint[]? GetUInt32Array(string path)
        {
            var value = GetNode(path);
            return value?.GetUInt32Array();
        }

        /// <inheritdoc />
        public virtual long[]? GetInt64Array(string path)
        {
            var value = GetNode(path);
            return value?.GetInt64Array();
        }

        /// <inheritdoc />
        public virtual ulong[]? GetUInt64Array(string path)
        {
            var value = GetNode(path);
            return value?.GetUInt64Array();
        }

        /// <inheritdoc />
        public virtual float[]? GetSingleArray(string path)
        {
            var value = GetNode(path);
            return value?.GetSingleArray();
        }

        /// <inheritdoc />
        public virtual double[]? GetDoubleArray(string path)
        {
            var value = GetNode(path);
            return value?.GetDoubleArray();
        }

        /// <inheritdoc />
        public virtual decimal[]? GetDecimalArray(string path)
        {
            var value = GetNode(path);
            return value?.GetDecimalArray();
        }

        /// <inheritdoc />
        public virtual string[]? GetStringArray(string path)
        {
            var value = GetNode(path);
            return value?.GetStringArray();
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual Config? GetConfig(string path)
        {
            var value = GetNode(path);
            if (Fallback != null)
            {
                var fallback = Fallback.GetConfig(path);
                if (value == null && fallback == null)
                    return null;
                if (value == null)
                    return fallback;
                if (fallback == null)
                    return new Config(new HoconRoot(value));

                return new Config(new HoconRoot(value)).WithFallback(fallback);
            }

            return value != null ? new Config(new HoconRoot(value)) : null;
        }

        /// <inheritdoc />
        public HoconValue? GetValue(string path)
        {
            return GetNode(path);
        }

        /// <inheritdoc />
        public virtual bool HasPath(string path)
        {
            return GetNode(path) != null;
        }

        /// <inheritdoc />
        public virtual IEnumerable<KeyValuePair<string, HoconValue>> AsEnumerable()
        {
            Config? current = this;
            var used = new HashSet<string>();
            while (current != null && current.Root != null)
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

        /// <inheritdoc />
        public override string ToString()
        {
            if (Root == null)
                return string.Empty;

            return Root.ToString();
        }
    }
}