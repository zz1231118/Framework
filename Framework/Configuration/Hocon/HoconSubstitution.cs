using System.Collections.Generic;

namespace Framework.Configuration.Hocon
{
    public class HoconSubstitution : IHoconElement, IMightBeAHoconObject
    {
        public HoconSubstitution(string path)
        {
            Path = path;
        }

        public string Path { get; set; }

        public HoconValue ResolvedValue { get; set; }

        public bool IsString()
        {
            return ResolvedValue.IsString();
        }

        public string GetString()
        {
            return ResolvedValue.GetString();
        }

        public bool IsArray()
        {
            return ResolvedValue.IsArray();
        }

        public IReadOnlyList<HoconValue> GetArray()
        {
            return ResolvedValue.GetArray();
        }

        public bool IsObject()
        {
            return ResolvedValue != null && ResolvedValue.IsObject();
        }

        public HoconObject GetObject()
        {
            return ResolvedValue.GetObject();
        }
    }
}
