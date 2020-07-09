using System;

namespace Framework.Data.Converters
{
    class TimeSpanConverter : IEntityConverter
    {
        public Type GetMappingType(Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            return typeof(string);
        }

        public object ConvertFrom(object value, Type targetType)
        {
            var timeSpan = (TimeSpan)value;
            return timeSpan.ToString(@"dd\.hh\:mm\:ss\.fff");
        }

        public object ConvertTo(object value, Type targetType)
        {
            if (value == null)
                return new TimeSpan();

            return TimeSpan.Parse(value as string);
        }
    }
}