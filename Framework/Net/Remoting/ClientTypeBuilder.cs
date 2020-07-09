using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Framework.Net.Remoting
{
    class ClientTypeBuilder
    {
        private Type _targetType;
        private TypeBuilder _typeBuilder;
        private ConstructorInfo _operationAttributeConstructor;
        private FieldInfo[] _operationAttributeFieldInfos;
        private PropertyInfo[] _operationAttributePropertyInfos;
        private MethodInfo _getCurrentMethodMethodInfo;
        private MethodInfo _requestMethodInfo;
        private MethodInfo _requestGenericMethodInfo;
        private Type _buildType;

        public ClientTypeBuilder(Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));
            if (!targetType.IsInterface)
                throw new ArgumentException("T must be an interface");

            _targetType = targetType;
        }

        private void InternalBuild()
        {
            AssemblyName assemblyName = new AssemblyName("DynamicClientAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            Type parentType = typeof(ClientBase);
            _typeBuilder = moduleBuilder.DefineType(string.Format("{0}.Client", _targetType.Namespace),
                TypeAttributes.Public | TypeAttributes.Class, parentType, new Type[] { _targetType });

            BuildTypeAttribute();
            BuildConstructor();

            BindingFlags bindAttr = BindingFlags.Public | BindingFlags.Static;
            _getCurrentMethodMethodInfo = typeof(MethodBase).GetMethod(nameof(MethodBase.GetCurrentMethod), bindAttr);

            //bindAttr &= ~(BindingFlags.Static | BindingFlags.Public);
            bindAttr = BindingFlags.Instance | BindingFlags.NonPublic;
            _requestMethodInfo = parentType.GetMethod("Request", bindAttr, new PowerBinder(false),
                new Type[] { typeof(MethodBase), typeof(object[]) }, new ParameterModifier[] { new ParameterModifier(2) });
            _requestGenericMethodInfo = parentType.GetMethod("Request", bindAttr, new PowerBinder(true),
                new Type[] { typeof(MethodBase), typeof(object[]) }, new ParameterModifier[] { new ParameterModifier(2) });

            Type attType = typeof(OperationContractAttribute);
            _operationAttributeConstructor = attType.GetConstructor(Type.EmptyTypes);
            bindAttr = BindingFlags.Instance | BindingFlags.Public;
            _operationAttributeFieldInfos = attType.GetFields(bindAttr);
            _operationAttributePropertyInfos = attType.GetProperties(bindAttr).Where(p => p.CanWrite).ToArray();

            var allInterface = _targetType.GetInterfaces();
            foreach (var type in allInterface)
            {
                foreach (var method in type.GetMethods())
                    BuildMethod(method);
            }
            foreach (var method in _targetType.GetMethods())
            {
                BuildMethod(method);
            }
        }
        private void BuildTypeAttribute()
        {
            var srvAttType = typeof(ServiceContractAttribute);
            var constructor = srvAttType.GetConstructor(Type.EmptyTypes);
            object[] atts = _targetType.GetCustomAttributes(srvAttType, false);
            CustomAttributeBuilder customeAttributeBuilder = null;
            if (atts.Length == 0)
            {
                customeAttributeBuilder = new CustomAttributeBuilder(constructor, new object[0]);
            }
            else
            {
                ServiceContractAttribute att = atts.Length == 0 ? null : atts[0] as ServiceContractAttribute;
                var bindAttr = BindingFlags.Instance | BindingFlags.Public;
                var attFieldInfos = srvAttType.GetFields(bindAttr);
                var attPropertyInfos = srvAttType.GetProperties(bindAttr).Where(p => p.CanWrite).ToArray();
                object[] attFieldValues = new object[attFieldInfos.Length];
                object[] attPropertyValues = new object[attPropertyInfos.Length];

                for (int i = 0; i < attFieldInfos.Length; i++)
                    attFieldValues[i] = attFieldInfos[i].GetValue(att);

                for (int i = 0; i < attPropertyInfos.Length; i++)
                    attPropertyValues[i] = attPropertyInfos[i].GetValue(att, null);

                customeAttributeBuilder = new CustomAttributeBuilder(constructor, new object[0],
                    attPropertyInfos, attPropertyValues, attFieldInfos, attFieldValues);
            }
            _typeBuilder.SetCustomAttribute(customeAttributeBuilder);
        }
        private void BuildConstructor()
        {
            Type clientEndpointType = typeof(ClientEndpoint);
            ConstructorInfo parentConstructor = typeof(ClientBase).GetConstructor(new Type[] { clientEndpointType });

            ConstructorBuilder constructorBuilder = _typeBuilder.DefineConstructor(MethodAttributes.Public,
                CallingConventions.Standard, new Type[] { clientEndpointType });
            constructorBuilder.DefineParameter(1, ParameterAttributes.None, "endpoint");
            ILGenerator iLGenerator = constructorBuilder.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Call, parentConstructor);
            iLGenerator.Emit(OpCodes.Ret);
        }
        private void BuildMethod(MethodInfo methodInfo)
        {
            bool hasReturn = methodInfo.ReturnType != typeof(void);
            ParameterInfo[] parameters = methodInfo.GetParameters();
            Type[] parameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                parameterTypes[i] = parameters[i].ParameterType;

            MethodAttributes methodAttribute = MethodAttributes.Public | MethodAttributes.Final |
                MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            MethodBuilder methodBuilder = _typeBuilder.DefineMethod(methodInfo.Name, methodAttribute,
                CallingConventions.Standard, methodInfo.ReturnType, parameterTypes);

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                methodBuilder.DefineParameter(i + 1, parameter.Attributes, parameter.Name);
            }

            BuildMethodAttribute(methodInfo, methodBuilder);
            ILGenerator iLGenerator = methodBuilder.GetILGenerator();
            if (hasReturn)
                iLGenerator.DeclareLocal(methodInfo.ReturnType);
            if (parameters.Length > 0)
                iLGenerator.DeclareLocal(typeof(object[]));

            iLGenerator.Emit(OpCodes.Nop);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Call, _getCurrentMethodMethodInfo);
            Ldc(iLGenerator, parameters.Length);

            iLGenerator.Emit(OpCodes.Newarr, typeof(object));
            if (parameters.Length > 0)
            {
                if (hasReturn)
                    iLGenerator.Emit(OpCodes.Stloc_1);
                else
                    iLGenerator.Emit(OpCodes.Stloc_0);

                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo parameter = parameters[i];
                    if (hasReturn) iLGenerator.Emit(OpCodes.Ldloc_1);
                    else iLGenerator.Emit(OpCodes.Ldloc_0);
                    Ldc(iLGenerator, i);
                    Ldarg(iLGenerator, i);
                    if (parameter.ParameterType.IsValueType)
                        iLGenerator.Emit(OpCodes.Box, parameter.ParameterType);
                    iLGenerator.Emit(OpCodes.Stelem_Ref);
                }
                if (hasReturn) iLGenerator.Emit(OpCodes.Ldloc_1);
                else iLGenerator.Emit(OpCodes.Ldloc_0);
            }
            if (hasReturn)
            {
                MethodInfo requestMethodInfo = _requestGenericMethodInfo.MakeGenericMethod(methodInfo.ReturnType);
                iLGenerator.Emit(OpCodes.Call, requestMethodInfo);
                iLGenerator.Emit(OpCodes.Stloc_0);
                iLGenerator.Emit(OpCodes.Ldloc_0);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Call, _requestMethodInfo);
            }

            iLGenerator.Emit(OpCodes.Ret);
        }
        private void BuildMethodAttribute(MethodInfo methodInfo, MethodBuilder methodBuilder)
        {
            object[] atts = methodInfo.GetCustomAttributes(typeof(OperationContractAttribute), false);
            CustomAttributeBuilder customeAttributeBuilder;
            if (atts.Length == 0)
            {
                customeAttributeBuilder = new CustomAttributeBuilder(_operationAttributeConstructor, new object[0]);
            }
            else
            {
                OperationContractAttribute att = atts.Length == 0 ? null : atts[0] as OperationContractAttribute;
                object[] attFieldValues = new object[_operationAttributeFieldInfos.Length];
                object[] attPropertyValues = new object[_operationAttributePropertyInfos.Length];

                for (int i = 0; i < _operationAttributeFieldInfos.Length; i++)
                    attFieldValues[i] = _operationAttributeFieldInfos[i].GetValue(att);

                for (int i = 0; i < _operationAttributePropertyInfos.Length; i++)
                    attPropertyValues[i] = _operationAttributePropertyInfos[i].GetValue(att, null);

                customeAttributeBuilder = new CustomAttributeBuilder(_operationAttributeConstructor, new object[0],
                    _operationAttributePropertyInfos, attPropertyValues, _operationAttributeFieldInfos, attFieldValues);
            }
            methodBuilder.SetCustomAttribute(customeAttributeBuilder);
        }
        private void Ldc(ILGenerator iLGenerator, int count)
        {
            switch (count)
            {
                case 0:
                    iLGenerator.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    iLGenerator.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    iLGenerator.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    iLGenerator.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    iLGenerator.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    iLGenerator.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    iLGenerator.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    iLGenerator.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    iLGenerator.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    iLGenerator.Emit(OpCodes.Ldc_I4_S, count);
                    break;
            }
        }
        private void Ldarg(ILGenerator iLGenerator, int index)
        {
            switch (index)
            {
                case 0:
                    iLGenerator.Emit(OpCodes.Ldarg_1);
                    break;
                case 1:
                    iLGenerator.Emit(OpCodes.Ldarg_2);
                    break;
                case 2:
                    iLGenerator.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    iLGenerator.Emit(OpCodes.Ldarg_S, index + 1);
                    break;
            }
        }
        public Type Build()
        {
            if (_buildType == null)
            {
                InternalBuild();
                _buildType = _typeBuilder.CreateTypeInfo();

                _targetType = null;
                _typeBuilder = null;
                _operationAttributeConstructor = null;
                _operationAttributeFieldInfos = null;
                _operationAttributePropertyInfos = null;
                _getCurrentMethodMethodInfo = null;
                _requestMethodInfo = null;
                _requestGenericMethodInfo = null;
            }

            return _buildType;
        }
        public ClientBase Create(ClientEndpoint endpoint)
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            var type = Build();
            return Activator.CreateInstance(type, new object[] { endpoint }) as ClientBase;
        }

        class PowerBinder : Binder
        {
            private readonly bool _genericMethod;

            public PowerBinder(bool genericMethod)
            {
                _genericMethod = genericMethod;
            }

            public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
            public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] names, out object state)
            {
                throw new NotImplementedException();
            }
            public override object ChangeType(object value, Type type, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
            public override void ReorderArgumentArray(ref object[] args, object state)
            {
                throw new NotImplementedException();
            }
            public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
            {
                return match.First(p => p.IsGenericMethod == _genericMethod);
            }
            public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
            {
                throw new NotImplementedException();
            }
        }
    }
}