using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleProtoGenerater.Utility
{
    public class ParamWrapper
    {
        private readonly Dictionary<string, List<string>> _kv = new Dictionary<string, List<string>>();

        public ParamWrapper(IEnumerable<string> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            foreach (var arg in collection)
            {
                if (string.IsNullOrWhiteSpace(arg))
                    continue;

                var ts = arg.Trim().TrimStart('-').Split('=');
                if (ts.Length != 2)
                    throw new ArgumentException();

                AddCommand(ts[0], ts[1]);
            }
        }

        private void AddCommand(string key, string value)
        {
            if (!_kv.TryGetValue(key, out List<string> list))
            {
                list = new List<string>();
                _kv[key] = list;
            }

            list.Add(value);
        }

        public IEnumerable<string> GetCommand(string key)
        {
            return _kv.TryGetValue(key, out List<string> list) ? list : Enumerable.Empty<string>();
        }
    }
}
