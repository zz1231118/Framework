using System;
using System.Reflection;

namespace Framework.Reflection
{
    /// <summary>
    /// 快速执行方法类
    /// </summary>
    public class FastInvokeMethod
    {
        private readonly MethodInfo _methodInfo;
        private readonly FastInvokeHandler _handler;

        public FastInvokeMethod(MethodInfo methodInfo, FastInvokeHandler handler)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _methodInfo = methodInfo;
            _handler = handler;
        }

        public MethodInfo MethodInfo => _methodInfo;
        public FastInvokeHandler Handler => _handler;
        /// <summary>
        /// 获取此方法的返回类型。
        /// </summary>
        public Type ReturnType
        {
            get { return MethodInfo.ReturnType; }
        }
        /// <summary>
        /// 获取声明该成员的类。
        /// </summary>
        public Type DeclaringType
        {
            get { return MethodInfo.DeclaringType; }
        }
        /// <summary>
        /// 获取用于获取 MemberInfo 的此实例的类对象。
        /// </summary>
        public Type ReflectedType
        {
            get { return MethodInfo.ReflectedType; }
        }

        /// <summary>
        /// 获取指定的方法或构造函数的参数。
        /// </summary>
        /// <returns></returns>
        public ParameterInfo[] GetParameters()
        {
            return MethodInfo.GetParameters();
        }

        public object Invoke(object target, params object[] args)
        {
            return _handler.Invoke(target, args);
        }
    }
}
