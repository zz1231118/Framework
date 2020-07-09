using System;

namespace Framework.JavaScript.Converters
{
    public class JsonGuidConverter : IJsonConverter
    {
        public object ConvertTo(Json value, Type conversionType)
        {
            if (value == Json.Null)
            {
                return null;
            }
            if (value is JsonValue jval && jval.Value is string other)
            {
                return Guid.Parse(other);
            }

            throw new InvalidCastException();
        }

        public Json ConvertFrom(object value, Type conversionType)
        {
            var guid = (Guid)value;
            return new JsonValue(guid.ToString("N"));
        }
    }
}
