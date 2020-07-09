using System;

namespace Framework.Data.Converters
{
    public interface IEntityConverter
    {
        Type GetMappingType(Type targetType);

        object ConvertFrom(object value, Type targetType);

        object ConvertTo(object value, Type targetType);
    }
}