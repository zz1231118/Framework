using System.Collections.Generic;

namespace Framework.Configuration.Hocon
{
    public interface IMightBeAHoconObject
    {
        bool IsObject();

        HoconObject GetObject();
    }

    public interface IHoconElement
    {
        bool IsString();

        string GetString();

        bool IsArray();

        IReadOnlyList<HoconValue> GetArray();
    }
}