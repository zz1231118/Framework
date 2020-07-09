using System;

namespace Framework.Data.Converters
{
    class EnumConverter : IEntityConverter
    {
        public Type GetMappingType(Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            return Enum.GetUnderlyingType(targetType);
        }

        public object ConvertFrom(object value, Type targetType)
        {
            var underlyingType = Enum.GetUnderlyingType(targetType);
            return System.Convert.ChangeType(value, underlyingType);
        }

        public object ConvertTo(object value, Type targetType)
        {
            return Enum.ToObject(targetType, value);
        }
    }
}