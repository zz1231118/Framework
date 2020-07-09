using System;

namespace Framework.Injection
{
    public class ServiceNotFoundException : Exception
    {
        public ServiceNotFoundException(string message)
            : base(message)
        { }
    }
}
