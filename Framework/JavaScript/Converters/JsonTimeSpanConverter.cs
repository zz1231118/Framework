using System;

namespace Framework.JavaScript.Converters
{
    public class JsonTimeSpanConverter : IJsonConverter
    {
        public object ConvertTo(Json value, Type conversionType)
        {
            if (value == Json.Null)
            {
                return null;
            }
            if (value is JsonValue jval && jval.Value is string other)
            {
                return TimeSpan.Parse(other);
            }

            throw new InvalidCastException();
        }

        public Json ConvertFrom(object value, Type conversionType)
        {
            var timeSpan = (TimeSpan)value;
            return new JsonValue(timeSpan.ToString());
        }
    }
}