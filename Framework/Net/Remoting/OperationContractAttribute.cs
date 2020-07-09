using System;

namespace Framework.Net.Remoting
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OperationContractAttribute : Attribute
    {
        public MethodType Method { get; set; }
    }
}