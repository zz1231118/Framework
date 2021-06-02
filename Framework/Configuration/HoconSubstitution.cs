using System.Collections.Generic;

namespace Framework.Configuration
{
    /// <inheritdoc />
    public class HoconSubstitution : IHoconElement, IMightBeAHoconObject
    {
        /// <inheritdoc />
        public HoconSubstitution(string path)
        {
            Path = path;
        }

        /// <inheritdoc />
        public string Path { get; set; }

        /// <inheritdoc />
        public HoconValue? ResolvedValue { get; set; }

        /// <inheritdoc />
        public bool IsString()
        {
            return ResolvedValue.IsString();
        }

        /// <inheritdoc />
        public string GetString()
        {
            return ResolvedValue.GetString();
        }

        /// <inheritdoc />
        public bool IsArray()
        {
            return ResolvedValue.IsArray();
        }

        /// <inheritdoc />
        public IReadOnlyList<HoconValue> GetArray()
        {
            return ResolvedValue.GetArray();
        }

        /// <inheritdoc />
        public bool IsObject()
        {
            return ResolvedValue != null && ResolvedValue.IsObject();
        }

        /// <inheritdoc />
        public HoconObject GetObject()
        {
            return ResolvedValue.GetObject();
        }
    }
}
