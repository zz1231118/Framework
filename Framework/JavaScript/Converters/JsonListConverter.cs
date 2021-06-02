using System;
using Framework.JavaScript.Utility;

namespace Framework.JavaScript.Converters
{
    public class JsonListConverter : IJsonConverter
    {
        public object ConvertTo(Json value, Type conversionType)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (conversionType == null)
                throw new ArgumentNullException(nameof(conversionType));
            if (!typeof(System.Collections.IList).IsAssignableFrom(conversionType))
                throw new ArgumentException("JsonListFormat 转换异常,传入的 [Type] 必须是实现了 ICollection<T> 的类型!");
            var itemType = MetaType.GetListItemType(conversionType);
            if (itemType == null)
                throw new InvalidCastException();
            if (value == Json.Null)
                return null;
            if (value is not JsonArray jarray)
                throw new InvalidCastException();

            var collection = (System.Collections.IList)Activator.CreateInstance(conversionType, true);
            foreach (var json in jarray)
            {
                var item = JsonSerializer.Deserialize(json, itemType);
                collection.Add(item);
            }
            return collection;
        }

        public Json ConvertFrom(object value, Type conversionType)
        {
            if (value == null)
                return Json.Null;
            if (value is not System.Collections.IList collection)
                throw new InvalidCastException();

            var array = new JsonArray();
            foreach (var item in collection)
            {
                var json = JsonSerializer.Serialize(item);
                array.Add(json);
            }
            return array;
        }
    }
}
