using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Framework.Injection
{
    public class ContainerBuilder : IReadOnlyList<ServiceDescriptor>
    {
        private readonly List<ServiceDescriptor> items = new List<ServiceDescriptor>();
        private bool isConstructed;
        private bool isAutowireEnabled;

        public ServiceDescriptor this[int index] => items[index];

        public int Count => items.Count;

        private void CheckConstructed()
        {
            if (isConstructed)
            {
                throw new InvalidOperationException("construted");
            }
        }

        public void EnableAutowired()
        {
            CheckConstructed();
            isAutowireEnabled = true;
        }

        public ServiceDescriptor GetServiceDescriptor(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            foreach (var descriptor in items)
            {
                if (descriptor.ServiceType == serviceType)
                {
                    return descriptor;
                }
            }
            //foreach (var descriptor in items)
            //{
            //    if (serviceType.IsAssignableFrom(descriptor.ServiceType))
            //    {
            //        return descriptor;
            //    }
            //}
            return null;
        }

        public ServiceDescriptor GetServiceDescriptor<TService>()
            where TService : class
        {
            return GetServiceDescriptor(typeof(TService));
        }

        public IContainer Build()
        {
            isConstructed = true;
            return new Container(this);
        }

        public ServiceDescriptor AddSingleton(Type serviceType, object instance)
        {
            CheckConstructed();
            var descriptor = GetServiceDescriptor(serviceType);
            if (descriptor != null) items.Remove(descriptor);

            descriptor = new ServiceDescriptor(serviceType, instance);
            items.Add(descriptor);
            return descriptor;
        }

        public ServiceDescriptor AddSingleton(object instance)
        {
            var serviceType = instance.GetType();
            return AddSingleton(serviceType, instance);
        }

        public ServiceDescriptor AddSingleton(Type serviceType, Type implementationType)
        {
            CheckConstructed();
            var descriptor = GetServiceDescriptor(serviceType);
            if (descriptor != null) items.Remove(descriptor);

            descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Singleton);
            items.Add(descriptor);
            return descriptor;
        }

        public ServiceDescriptor AddSingleton(Type serviceType)
        {
            return AddSingleton(serviceType, serviceType);
        }

        public ServiceDescriptor AddSingleton<TService>()
            where TService : class
        {
            return AddSingleton(typeof(TService), typeof(TService));
        }

        public ServiceDescriptor AddSingleton<TService, TImplementation>()
             where TService : class
             where TImplementation : TService
        {
            return AddSingleton(typeof(TService), typeof(TImplementation));
        }

        public ServiceDescriptor AddSingleton<TService>(object instance)
            where TService : class
        {
            return AddSingleton(typeof(TService), instance);
        }

        public ServiceDescriptor AddSingleton<TService>(Func<IContainer, TService> factory)
            where TService : class
        {
            CheckConstructed();
            var serviceType = typeof(TService);
            var descriptor = GetServiceDescriptor(serviceType);
            if (descriptor != null) items.Remove(descriptor);

            descriptor = new ServiceDescriptor(serviceType, factory, ServiceLifetime.Singleton);
            items.Add(descriptor);
            return descriptor;
        }

        public ServiceDescriptor AddTransient(Type serviceType, Type implementationType)
        {
            CheckConstructed();
            var descriptor = GetServiceDescriptor(serviceType);
            if (descriptor != null) items.Remove(descriptor);

            descriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
            items.Add(descriptor);
            return descriptor;
        }

        public ServiceDescriptor AddTransient(Type serviceType)
        {
            return AddTransient(serviceType, serviceType);
        }

        public ServiceDescriptor AddTransient<TService>()
            where TService : class
        {
            return AddTransient(typeof(TService), typeof(TService));
        }

        public ServiceDescriptor AddTransient<TService, TImplementation>()
             where TService : class
             where TImplementation : TService
        {
            return AddTransient(typeof(TService), typeof(TImplementation));
        }

        public ServiceDescriptor AddTransient<TService>(Func<IContainer, TService> factory)
            where TService : class
        {
            CheckConstructed();
            var serviceType = typeof(TService);
            var descriptor = GetServiceDescriptor(serviceType);
            if (descriptor != null) items.Remove(descriptor);

            descriptor = new ServiceDescriptor(serviceType, factory, ServiceLifetime.Transient);
            items.Add(descriptor);
            return descriptor;
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        sealed class Container : IContainer
        {
            private readonly ContainerBuilder builder;

            public Container(ContainerBuilder builder)
            {
                this.builder = builder;
            }

            private static ConstructorInfo GetAvailableConstructor(Type type)
            {
                var bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var constructors = type.GetConstructors(bindingAttr);
                if (constructors.Length == 1)
                {
                    return constructors[0];
                }
                if (constructors.Length > 1)
                {
                    return type.GetConstructor(bindingAttr, null, Type.EmptyTypes, null);
                }

                return null;
            }

            private static List<ServiceFieldMetadata> GetFields(Type type)
            {
                var fields = new List<ServiceFieldMetadata>();
                var objectType = typeof(object);
                var bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

                do
                {
                    foreach (var field in type.GetFields(bindingAttr))
                    {
                        var autowired = field.GetCustomAttribute<AutowiredAttribute>();
                        if (autowired != null)
                        {
                            var metadata = new ServiceFieldMetadata();
                            metadata.FieldInfo = field;
                            metadata.Options = autowired.Options;
                            fields.Add(metadata);
                        }
                    }

                    type = type.BaseType;
                } while (type != objectType);
                return fields;
            }

            private ServiceMetadata GetServiceMetadata(Type implementationType)
            {
                var metadata = new ServiceMetadata();
                metadata.Constructor = GetAvailableConstructor(implementationType);
                if (builder.isAutowireEnabled)
                {
                    metadata.Fields = GetFields(implementationType);
                    metadata.IsAutowired = metadata.Fields.Count > 0;
                }
                return metadata;
            }

            private void Autowire(ServiceDescriptor descriptor, object obj)
            {
                foreach (var metadata in descriptor.Metadata.Fields)
                {
                    object value;
                    switch (metadata.Options)
                    {
                        case Automatic.Optional:
                            value = Optional(metadata.FieldInfo.FieldType);
                            break;
                        case Automatic.Required:
                            value = Required(metadata.FieldInfo.FieldType);
                            break;
                        default:
                            throw new InvalidOperationException($"type:{obj.GetType()} field:{metadata.FieldInfo} unknown {nameof(Automatic)}: {metadata.Options}");
                    }

                    metadata.SetValue(obj, value);
                }
            }

            private Func<IContainer, object> CreateImplementationFactory(ServiceDescriptor descriptor, List<Type> callings)
            {
                return new Func<IContainer, object>((container) =>
                {
                    if (descriptor.Metadata.Constructor == null)
                    {
                        throw new InvalidOperationException("not supported service constructor: " + descriptor.ServiceType.FullName);
                    }

                    var callingInitializing = false;
                    var constructor = descriptor.Metadata.Constructor;
                    var parameters = constructor.GetParameters();
                    var arguments = new object[parameters.Length];
                    var containerType = typeof(IContainer);
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];
                        var parameterType = parameter.ParameterType;
                        if (parameterType == containerType)
                        {
                            arguments[i] = container;
                            continue;
                        }
                        if (!callingInitializing)
                        {
                            callingInitializing = true;
                            callings ??= new List<Type>();
                            callings.Add(descriptor.ServiceType);
                        }
                        if (callings.Contains(parameterType))
                        {
                            var text = string.Join(Environment.NewLine + "-> ", callings.Select(p => p.FullName));
                            throw new InvalidOperationException("circular reference : " + text);
                        }

                        arguments[i] = GetService(parameter.ParameterType, new List<Type>(callings));
                    }

                    return constructor.Invoke(arguments);
                });
            }

            private object GetService(Type serviceType, List<Type> callings)
            {
                var descriptor = builder.GetServiceDescriptor(serviceType);
                if (descriptor == null)
                {
                    //service not found.
                    return null;
                }

                switch (descriptor.Lifetime)
                {
                    case ServiceLifetime.Singleton:
                        if (descriptor.ImplementationInstance == null)
                        {
                            lock (descriptor)
                            {
                                if (descriptor.ImplementationInstance == null)
                                {
                                    descriptor.Metadata ??= GetServiceMetadata(descriptor.ImplementationType);
                                    var implementationFactory = descriptor.ImplementationFactory ?? CreateImplementationFactory(descriptor, callings);
                                    descriptor.ImplementationInstance = implementationFactory(this);
                                    if (descriptor.Metadata.IsAutowired) Autowire(descriptor, descriptor.ImplementationInstance);
                                }
                            }
                        }
                        return descriptor.ImplementationInstance;
                    case ServiceLifetime.Transient:
                        if (descriptor.ImplementationFactory == null)
                        {
                            lock (descriptor)
                            {
                                if (descriptor.ImplementationFactory == null)
                                {
                                    descriptor.ImplementationFactory = CreateImplementationFactory(descriptor, callings);
                                }
                            }
                        }

                        var instance = descriptor.ImplementationFactory(this);
                        descriptor.Metadata ??= GetServiceMetadata(instance.GetType());
                        if (descriptor.Metadata.IsAutowired) Autowire(descriptor, instance);
                        return instance;
                    default:
                        throw new InvalidOperationException("not supported lifetime: " + descriptor.Lifetime.ToString());
                }
            }

            public object Optional(Type serviceType)
            {
                if (serviceType == null)
                    throw new ArgumentNullException(nameof(serviceType));

                return GetService(serviceType, null);
            }

            public object Required(Type serviceType)
            {
                if (serviceType == null)
                    throw new ArgumentNullException(nameof(serviceType));

                var service = GetService(serviceType, null);
                if (service == null)
                {
                    throw new ServiceNotFoundException(serviceType.FullName);
                }

                return service;
            }
        }
    }
}
