using System;
using System.Collections.Generic;

namespace Framework.Configuration
{
    internal class HoconLiteral : IHoconElement
    {
        private readonly string value;

        public HoconLiteral(string value)
        {
            this.value = value;
        }

        public bool IsString()
        {
            return true;
        }

        public string GetString()
        {
            return value;
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
            return value;
        }
    }
}
