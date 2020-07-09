using System;
using Framework.JavaScript;

namespace Framework.Data.Converters
{
    public class JsonEntityConverter<T> : IEntityConverter
    {
        public Type GetMappingType(Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            return typeof(string);
        }

        public object ConvertFrom(object value, Type targetType)
        {
            var json = JsonSerializer.Serialize(value);
            return json.ToString();
        }

        public object ConvertTo(object value, Type targetType)
        {
            if (string.IsNullOrEmpty(value as string))
                return null;

            var json = Json.Parse(value as string);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
