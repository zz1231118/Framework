using System;
using System.Reflection;

namespace Framework
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class PropertyAttribute : Attribute
    {
        /// <summary>
        /// 属性信息
        /// </summary>
        public PropertyInfo PropertyInfo { get; internal set; }

        /// <summary>
        /// 获取成员返回类型
        /// </summary>
        public Type PropertyType
        {
            get
            {
                if (PropertyInfo == null)
                    throw new InvalidOperationException("PropertyInfo is null!");

                return PropertyInfo.PropertyType;
            }
        }

        /// <summary>
        /// 获取声明该成员的类
        /// </summary>
        public Type DeclaringType
        {
            get
            {
                if (PropertyInfo == null)
                    throw new InvalidOperationException("PropertyInfo is null!");

                return PropertyInfo.DeclaringType;
            }
        }

        /// <summary>
        /// 获取用于获取此成员的此实例的类对象
        /// </summary>
        public Type ReflectedType
        {
            get
            {
                if (PropertyInfo == null)
                    throw new InvalidOperationException("PropertyInfo is null!");

                return PropertyInfo.ReflectedType;
            }
        }

        /// <summary>
        /// 是否支持可读
        /// </summary>
        public bool CanRead
        {
            get
            {
                if (PropertyInfo == null)
                    throw new InvalidOperationException("PropertyInfo is null!");

                return PropertyInfo.CanRead;
            }
        }

        /// <summary>
        /// 是否支持可写
        /// </summary>
        public bool CanWrite
        {
            get
            {
                if (PropertyInfo == null)
                    throw new InvalidOperationException("PropertyInfo is null!");

                return PropertyInfo.CanWrite;
            }
        }

        public void SetValue(object obj, object value)
        {
            if (PropertyInfo == null)
                throw new InvalidOperationException("PropertyInfo is null!");

            SetValue(obj, value, null);
        }

        public virtual void SetValue(object obj, object value, object[] index)
        {
            if (PropertyInfo == null)
                throw new InvalidOperationException("PropertyInfo is null!");

            PropertyInfo.SetValue(obj, value, index);
        }

        public object GetValue(object obj)
        {
            if (PropertyInfo == null)
                throw new InvalidOperationException("PropertyInfo is null!");

            return GetValue(obj, null);
        }

        public virtual object GetValue(object obj, object[] index)
        {
            if (PropertyInfo == null)
                throw new InvalidOperationException("PropertyInfo is null!");

            return PropertyInfo.GetValue(obj, index);
        }
    }
}