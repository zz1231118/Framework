using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Framework.Reflection
{
    /// <summary>
    /// 快速执行方法工厂
    /// </summary>
    public static class ILMethodFactory
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, FastInvokeMethod[]>> _kv;
        private static readonly Func<Type, Dictionary<string, FastInvokeMethod[]>> _factory;
        private static Func<Type, MethodInfo[]> _getMethodFactory;

        static ILMethodFactory()
        {
            _kv = new ConcurrentDictionary<Type, Dictionary<string, FastInvokeMethod[]>>();
            _factory = new Func<Type, Dictionary<string, FastInvokeMethod[]>>(CreateHandlers);
        }

        public static void SetGetMethods(Func<Type, MethodInfo[]> factory)
        {
            _getMethodFactory = factory;
        }
        /// <summary>
        /// 创建 方法调用 委托
        /// </summary>
        public static FastInvokeHandler CreateMethodHandler(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            DynamicMethod dm = new DynamicMethod(nameof(DynamicMethod), typeof(object),
                new Type[] { typeof(object), typeof(object[]) });
            ILGenerator il = dm.GetILGenerator();
            ParameterInfo[] parameters = methodInfo.GetParameters();
            Type[] parameterTypes = new Type[parameters.Length];
            LocalBuilder[] localBuilders = new LocalBuilder[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                Type pType;
                if (parameters[i].ParameterType.IsByRef)
                    pType = parameters[i].ParameterType.GetElementType();
                else
                    pType = parameters[i].ParameterType;

                parameterTypes[i] = pType;
                localBuilders[i] = il.DeclareLocal(pType, true);
            }
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                EmitFastInt(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitCastToReference(il, parameterTypes[i]);
                il.Emit(OpCodes.Stloc, localBuilders[i]);
            }
            if (!methodInfo.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, methodInfo.DeclaringType);
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldloca_S, localBuilders[i]);
                }
                else
                {
                    il.Emit(OpCodes.Ldloc, localBuilders[i]);
                }
            }
            if (!methodInfo.IsStatic)
            {
                il.EmitCall(OpCodes.Callvirt, methodInfo, null);
            }
            else
            {
                il.EmitCall(OpCodes.Call, methodInfo, null);
            }
            if (methodInfo.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else if (methodInfo.ReturnType.IsValueType)
            {
                il.Emit(OpCodes.Box, methodInfo.ReturnType);
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    EmitFastInt(il, i);
                    il.Emit(OpCodes.Ldloc, localBuilders[i]);
                    if (localBuilders[i].LocalType.IsValueType)
                        il.Emit(OpCodes.Box, localBuilders[i].LocalType);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }
            il.Emit(OpCodes.Ret);
            return (FastInvokeHandler)dm.CreateDelegate(typeof(FastInvokeHandler));
        }
        /// <summary>
        /// 获取或创建指定类型的指定名称的方法动态委托
        /// </summary>
        public static bool TryGetOrCreate(Type type, string name, out FastInvokeMethod[] handlers)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name == string.Empty)
                throw new ArgumentException(nameof(name));

            var cache = _kv.GetOrAdd(type, _factory);
            return cache.TryGetValue(name, out handlers);
        }
        /// <summary>
        /// 获取或创建指定类型的指定名称的方法动态委托
        /// </summary>
        public static bool TryGetOrCreate<T>(string name, out FastInvokeMethod[] handlers)
        {
            return TryGetOrCreate(typeof(T), name, out handlers);
        }

        private static Dictionary<string, FastInvokeMethod[]> CreateHandlers(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var kv = new Dictionary<string, List<FastInvokeMethod>>();
            var methods = _getMethodFactory != null
                ? _getMethodFactory(type)
                : type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var handler = CreateMethodHandler(method);
                if (!kv.TryGetValue(method.Name, out List<FastInvokeMethod> lt))
                    lt = kv[method.Name] = new List<FastInvokeMethod>();

                lt.Add(new FastInvokeMethod(method, handler));
            }
            return kv.ToDictionary(k => k.Key, v => v.Value.ToArray());
        }
        private static void EmitCastToReference(ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }
        private static void EmitFastInt(ILGenerator ilGenerator, int value)
        {
            switch (value)
            {
                case -1:
                    ilGenerator.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0:
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    ilGenerator.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    ilGenerator.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    ilGenerator.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    ilGenerator.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    ilGenerator.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    ilGenerator.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    ilGenerator.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    ilGenerator.Emit(OpCodes.Ldc_I4_8);
                    return;
            }
            if (value > -129 && value < 128)
            {
                ilGenerator.Emit(OpCodes.Ldc_I4_S, (SByte)value);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldc_I4, value);
            }
        }
    }
}