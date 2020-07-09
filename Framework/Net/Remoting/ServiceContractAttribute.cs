using System;

namespace Framework.Net.Remoting
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ServiceContractAttribute : Attribute
    {
        public ConcurrencyMode ConcurrencyMode { get; set; }
        public Type CallbackContract { get; set; }
    }
}