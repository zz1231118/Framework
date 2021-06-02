using System;

namespace Framework.Injection
{
    public class ServiceDescriptor
    {
        private readonly Type serviceType;
        private readonly ServiceLifetime lifetime;
        private Type implementationType;
        private object implementationInstance;
        private Func<IContainer, object> implementationFactory;

        private ServiceDescriptor(Type serviceType, ServiceLifetime lifetime)
        {
            this.serviceType = serviceType;
            this.lifetime = lifetime;
        }

        public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
            : this(serviceType, lifetime)
        {
            this.implementationType = implementationType;
        }

        public ServiceDescriptor(Type serviceType, object implementationInstance)
            : this(serviceType, ServiceLifetime.Singleton)
        {
            this.implementationInstance = implementationInstance;
        }

        public ServiceDescriptor(Type serviceType, Func<IContainer, object> implementationFactory, ServiceLifetime lifetime)
            : this(serviceType, lifetime)
        {
            this.implementationFactory = implementationFactory;
        }

        internal ServiceMetadata Metadata { get; set; }

        public ServiceLifetime Lifetime => lifetime;

        public Type ServiceType => serviceType;

        public Type ImplementationType
        {
            get
            {
                if (implementationType == null)
                {
                    if (implementationInstance != null) implementationType = implementationInstance.GetType();
                    else throw new InvalidOperationException("get implementation type error.");
                }

                return implementationType;
            }
        }

        public object ImplementationInstance
        {
            get => implementationInstance;
            set => implementationInstance = value;
        }

        public Func<IContainer, object> ImplementationFactory
        {
            get => implementationFactory;
            set => implementationFactory = value;
        }
    }
}
