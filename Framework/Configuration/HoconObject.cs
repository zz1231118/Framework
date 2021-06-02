using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.Configuration
{
    /// <inheritdoc />
    public class HoconObject : IHoconElement
    {
        /// <inheritdoc />
        public HoconObject()
        {
            Items = new Dictionary<string, HoconValue>();
        }

        /// <inheritdoc />
        public IDictionary<string, object> Unwrapped
        {
            get
            {
                return Items.ToDictionary(k => k.Key, v => (object?)v.Value.GetObject()?.Unwrapped ?? v.Value);
            }
        }

        /// <inheritdoc />
        public Dictionary<string, HoconValue> Items { get; private set; }

        /// <inheritdoc />
        public bool IsString()
        {
            return false;
        }

        /// <inheritdoc />
        public string GetString()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public bool IsArray()
        {
            return false;
        }

        /// <inheritdoc />
        public IReadOnlyList<HoconValue> GetArray()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public HoconValue GetKey(string key)
        {
            Items.TryGetValue(key, out HoconValue value);
            return value;
        }

        /// <inheritdoc />
        public HoconValue GetOrCreateKey(string key)
        {
            if (!Items.TryGetValue(key, out HoconValue value))
            {
                value = new HoconValue();
                Items.Add(key, value);
            }
            return value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToString(0);
        }

        /// <inheritdoc />
        public string ToString(int indent)
        {
            var i = new string(' ', indent * 2);
            var sb = new StringBuilder();
            foreach (var kvp in Items)
            {
                string key = QuoteIfNeeded(kvp.Key);
                sb.AppendFormat("{0}{1} : {2}\r\n", i, key, kvp.Value.ToString(indent));
            }
            return sb.ToString();
        }

        private string QuoteIfNeeded(string text)
        {
            if (text.ToCharArray().Intersect(" \t".ToCharArray()).Any())
            {
                return "\"" + text + "\"";
            }
            return text;
        }

        /// <inheritdoc />
        public void Merge(HoconObject other)
        {
            var thisItems = Items;
            var otherItems = other.Items;

            foreach (var otherItem in otherItems)
            {
                if (thisItems.ContainsKey(otherItem.Key))
                {
                    var thisItem = thisItems[otherItem.Key];
                    if (thisItem.IsObject() && otherItem.Value.IsObject())
                    {
                        thisItem.GetObject().Merge(otherItem.Value.GetObject());
                    }
                }
                else
                {
                    Items.Add(otherItem.Key, otherItem.Value);
                }
            }
        }
    }
}
