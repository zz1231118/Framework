using System;
using System.Collections.Generic;

namespace Framework.Configuration.Hocon
{
    public class HoconArray : List<HoconValue>, IHoconElement
    {
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
            return true;
        }

        public IReadOnlyList<HoconValue> GetArray()
        {
            return this;
        }

        public override string ToString()
        {
            return "[" + string.Join(",", this) + "]";
        }
    }
}
