using System.Collections.Generic;

namespace Framework.Configuration
{
    /// <inheritdoc />
    public interface IMightBeAHoconObject
    {
        /// <inheritdoc />
        bool IsObject();

        /// <inheritdoc />
        HoconObject GetObject();
    }

    /// <inheritdoc />
    public interface IHoconElement
    {
        /// <inheritdoc />
        bool IsString();

        /// <inheritdoc />
        string GetString();

        /// <inheritdoc />
        bool IsArray();

        /// <inheritdoc />
        IReadOnlyList<HoconValue> GetArray();
    }
}