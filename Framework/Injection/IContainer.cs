using System;

namespace Framework.Injection
{
    public interface IContainer
    {
        object Optional(Type serviceType);

        object Required(Type serviceType);
    }
}
