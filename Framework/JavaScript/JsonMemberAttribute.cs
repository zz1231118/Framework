using System;
using System.Collections.Concurrent;
using System.Reflection;
using Framework.JavaScript.Converters;

namespace Framework.JavaScript
{
    /// <summary>
    /// 提供 成员属性到 MyJson 的 特性标注
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonMemberAttribute : Attribute
    {
        private static readonly ConcurrentDictionary<Type, IJsonConverter> _kvDataFormat = new ConcurrentDictionary<Type, IJsonConverter>();

        private static readonly Func<Type, IJsonConverter> _dataFormatFactory = key => (IJsonConverter)Activator.CreateInstance(key, true);
        private string name;

        public JsonMemberAttribute()
        { }

        public JsonMemberAttribute(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            this.name = name;
        }

        public PropertyInfo PropertyInfo { get; internal set; }

        public bool CanRead => PropertyInfo.CanRead;

        public bool CanWrite => PropertyInfo.CanWrite;

        public Type PropertyType => PropertyInfo.PropertyType;

        public Type DeclaringType => PropertyInfo.DeclaringType;

        /// <summary>
        /// 显示名
        /// </summary>
        public string Name
        {
            get
            {
                if (name == null)
                {
                    if (PropertyInfo != null)
                        name = PropertyInfo.Name;
                }
                return name;
            }
            set { name = value; }
        }

        /// <summary>
        /// 显示顺序
        /// </summary>
        public int ShowIndex { get; set; }

        /// <summary>
        /// DataFormat 对象 Type
        /// </summary>
        public Type ConverterType { get; set; }

        /// <summary>
        /// 获取指定 Type 类型的 IDataFormat 对象
        /// </summary>
        protected static IJsonConverter GetFormatTarget(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!typeof(IJsonConverter).IsAssignableFrom(type))
                throw new ArgumentException("type not is assignable from IDataFormat");

            return _kvDataFormat.GetOrAdd(type, _dataFormatFactory);
        }

        /// <summary>
        /// 获取指定类型的值
        /// </summary>
        /// <param name="obj">欲获取的对象</param>
        /// <param name="checkFormat">是否检测 FormatType</param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <returns></returns>
        public Json GetValue(object obj, bool checkFormat = true)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (!CanRead)
                throw new InvalidOperationException("未找到属性设置方法!");

            object result = PropertyInfo.GetValue(obj);
            if (result != null)
            {
                if (checkFormat && ConverterType != null)
                {
                    if (!typeof(IJsonConverter).IsAssignableFrom(ConverterType))
                    {
                        throw new JsonFormatException(string.Format("Type.Name=[{0}] PropertyType.Name=[{1}] FormatType.Name=[{2}] FormatType错误.",
                            obj.GetType().Name, PropertyType.Name, ConverterType.Name));
                    }
                    var dataFormat = GetFormatTarget(ConverterType);
                    result = dataFormat.ConvertFrom(result, PropertyType);
                }
            }
            return Json.ConvertTo(result);
        }

        /// <summary>
        /// 设置指定类型的值
        /// </summary>
        /// <param name="obj">欲设置的对象</param>
        /// <param name="value">欲设置的值</param>
        /// <param name="checkFormat">是否检测 FormatType</param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="Framework.Jsons.JsonFormatException"></exception>
        /// <exception cref="Framework.Jsons.JsonException"></exception>
        /// <returns></returns>
        public object SetValue(object obj, Json value, bool checkFormat = true)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (!CanWrite)
                throw new InvalidOperationException("未找到属性设置方法!");

            object result = null;
            if (value != null)
            {
                if (checkFormat && ConverterType != null)
                {
                    if (!typeof(IJsonConverter).IsAssignableFrom(ConverterType))
                    {
                        throw new JsonFormatException(string.Format("Type.Name=[{0}] PropertyType.Name=[{1}] FormatType.Name=[{2}] FormatType错误.",
                            obj.GetType().Name, PropertyType.Name, ConverterType.Name));
                    }
                    var dataFormat = GetFormatTarget(ConverterType);
                    result = dataFormat.ConvertTo(value, PropertyType);
                }
                else
                {
                    result = Json.ChangeType(value, PropertyType);
                }
            }
            PropertyInfo.SetValue(obj, result);
            return result;
        }
    }
}