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
        private static readonly ConcurrentDictionary<Type, IJsonConverter> jsonConverters = new ConcurrentDictionary<Type, IJsonConverter>();
        private static readonly Func<Type, IJsonConverter> jsonConverterFactory = key => (IJsonConverter)Activator.CreateInstance(key, true);
        private PropertyInfo? propertyInfo;
        private Type? converterType;
        private string? name;

        /// <inheritdoc />
        public JsonMemberAttribute()
        { }

        /// <inheritdoc />
        public JsonMemberAttribute(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            this.name = name;
        }

        /// <inheritdoc />
        public PropertyInfo PropertyInfo
        {
            get => propertyInfo ?? throw new InvalidOperationException("PropertyInfo should not be null");
            internal set => propertyInfo = value;
        }

        /// <inheritdoc />
        public bool CanRead
        {
            get 
            {
                if (propertyInfo == null)
                    throw new InvalidOperationException("PropertyInfo should not be null");

                return propertyInfo.CanRead;
            }
        }

        /// <inheritdoc />
        public bool CanWrite
        {
            get
            {
                if (propertyInfo == null)
                    throw new InvalidOperationException("PropertyInfo should not be null");

                return propertyInfo.CanWrite;
            }
        }

        /// <inheritdoc />
        public Type PropertyType
        {
            get
            {
                if (propertyInfo == null)
                    throw new InvalidOperationException("PropertyInfo should not be null");

                return propertyInfo.PropertyType;
            }
        }

        /// <inheritdoc />
        public Type DeclaringType
        {
            get
            {
                if (propertyInfo == null)
                    throw new InvalidOperationException("PropertyInfo should not be null");

                return propertyInfo.DeclaringType;
            }
        }

        /// <summary>
        /// 显示名
        /// </summary>
        public string Name
        {
            get
            {
                if (name == null)
                {
                    if (propertyInfo == null)
                        throw new InvalidOperationException("PropertyInfo should not be null");

                    name = propertyInfo.Name;
                }
                return name;
            }
            set { name = value; }
        }

        /// <summary>
        /// DataFormat 对象 Type
        /// </summary>
        public Type? ConverterType
        { 
            get => converterType; 
            set => converterType = value;
        }

        /// <summary>
        /// 获取指定 Type 类型的 IDataFormat 对象
        /// </summary>
        protected static IJsonConverter GetFormatTarget(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!typeof(IJsonConverter).IsAssignableFrom(type))
                throw new ArgumentException("type not is assignable from IDataFormat");

            return jsonConverters.GetOrAdd(type, jsonConverterFactory);
        }

        /// <summary>
        /// 获取指定类型的值
        /// </summary>
        /// <param name="obj">欲获取的对象</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <returns></returns>
        public Json GetValue(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (propertyInfo == null)
                throw new InvalidOperationException("PropertyInfo should not be null");
            if (!propertyInfo.CanRead)
                throw new InvalidOperationException("未找到属性设置方法!");

            var result = propertyInfo.GetValue(obj);
            if (result != null)
            {
                if (converterType != null)
                {
                    if (!typeof(IJsonConverter).IsAssignableFrom(converterType))
                    {
                        throw new JsonFormatException($"Type.Name=[{obj.GetType().Name}] PropertyType.Name=[{PropertyType.Name}] FormatType.Name=[{converterType.Name}] FormatType错误.");
                    }

                    var dataFormat = GetFormatTarget(converterType);
                    result = dataFormat.ConvertFrom(result, PropertyType);
                }
            }

            return Json.ConvertFrom(result);
        }

        /// <summary>
        /// 设置指定类型的值
        /// </summary>
        /// <param name="obj">欲设置的对象</param>
        /// <param name="value">欲设置的值</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="JsonException"></exception>
        /// <exception cref="JsonFormatException"></exception>
        /// <returns></returns>
        public object? SetValue(object obj, Json value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (propertyInfo == null)
                throw new InvalidOperationException("PropertyInfo should not be null");
            if (!propertyInfo.CanWrite)
                throw new InvalidOperationException("未找到属性设置方法!");

            object? result = null;
            if (value != null)
            {
                if (converterType != null)
                {
                    if (!typeof(IJsonConverter).IsAssignableFrom(converterType))
                    {
                        throw new JsonFormatException($"Type.Name=[{obj.GetType().Name}] PropertyType.Name=[{PropertyType.Name}] FormatType.Name=[{converterType.Name}] FormatType错误.");
                    }

                    var dataFormat = GetFormatTarget(converterType);
                    result = dataFormat.ConvertTo(value, PropertyType);
                }
                else
                {
                    result = Json.ConvertTo(value, PropertyType);
                }
            }

            propertyInfo.SetValue(obj, result);
            return result;
        }
    }
}