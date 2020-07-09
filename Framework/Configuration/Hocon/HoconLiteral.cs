using System;
using System.Collections.Generic;

namespace Framework.Configuration.Hocon
{
    public class HoconLiteral : IHoconElement
    {
        public string Value { get; set; }

        public bool IsString()
        {
            return true;
        }

        public string GetString()
        {
            return Value;
        }

        public bool IsArray()
        {
            return false;
        }

        public IReadOnlyList<HoconValue> GetArray()
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
