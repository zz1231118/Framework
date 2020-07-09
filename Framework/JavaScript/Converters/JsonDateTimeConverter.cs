using System;

namespace Framework.JavaScript.Converters
{
    public class JsonDateTimeConverter : IJsonConverter
    {
        public object ConvertTo(Json value, Type conversionType)
        {
            if (value == Json.Null)
            {
                return null;
            }
            if (value is JsonValue jval && jval.Value is string other)
            {
                return DateTime.Parse(other);
            }

            throw new InvalidCastException();
        }

        public Json ConvertFrom(object value, Type conversionType)
        {
            var dateTime = (DateTime)value;
            return new JsonValue(dateTime.ToString("yyyy-MM-dd HH:mm:ss.fffff"));
        }
    }
}
