using System;

namespace Framework.Injection
{
    public static class ContainerExtensions
    {
        public static T Optional<T>(this IContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            return (T)container.Optional(typeof(T));
        }

        public static T Required<T>(this IContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            return (T)container.Required(typeof(T));
        }
    }
}
