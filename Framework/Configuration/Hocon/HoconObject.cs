using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.Configuration.Hocon
{
    public class HoconObject : IHoconElement
    {
        public HoconObject()
        {
            Items = new Dictionary<string, HoconValue>();
        }

        public IDictionary<string, object> Unwrapped
        {
            get
            {
                return Items.ToDictionary(k => k.Key, v => (object)v.Value.GetObject()?.Unwrapped ?? v.Value);
            }
        }

        public Dictionary<string, HoconValue> Items { get; private set; }

        public bool IsString()
        {
            return false;
        }

        public string GetString()
        {
            throw new NotSupportedException();
        }

        public bool IsArray()
        {
            return false;
        }

        public IReadOnlyList<HoconValue> GetArray()
        {
            throw new NotSupportedException();
        }

        public HoconValue GetKey(string key)
        {
            Items.TryGetValue(key, out HoconValue value);
            return value;
        }

        public HoconValue GetOrCreateKey(string key)
        {
            if (!Items.TryGetValue(key, out HoconValue value))
            {
                value = new HoconValue();
                Items.Add(key, value);
            }
            return value;
        }

        public override string ToString()
        {
            return ToString(0);
        }

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
