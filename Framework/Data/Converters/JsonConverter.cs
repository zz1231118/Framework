﻿using System;
using Framework.JavaScript;
using Framework.JavaScript.Converters;

namespace Framework.Data.Converters
{
    public class JsonConverter : IEntityConverter
    {
        public Type GetMappingType(Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            return typeof(string);
        }

        public object ConvertFrom(object value, Type targetType)
        {
            return value.ToString();
        }

        public object ConvertTo(object value, Type targetType)
        {
            if (value is not string text) throw new InvalidCastException();
            else return Json.Parse(text);
        }
    }

    public class JsonConverter<TConverter> : IEntityConverter
        where TConverter : IJsonConverter, new()
    {
        private IJsonConverter? _converter;

        private IJsonConverter GetConverter()
        {
            if (_converter == null)
                _converter = Activator.CreateInstance<TConverter>();

            return _converter;
        }

        public Type GetMappingType(Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            return typeof(string);
        }

        public object ConvertFrom(object value, Type targetType)
        {
            var converter = GetConverter();
            var json = converter.ConvertFrom(value, targetType);
            return json.ToString();
        }

        public object ConvertTo(object value, Type targetType)
        {
            if (value is not string text)
            {
                throw new InvalidCastException();
            }

            var json = Json.Parse(text);
            var converter = GetConverter();
            return converter.ConvertTo(json, targetType);
        }
    }
}
