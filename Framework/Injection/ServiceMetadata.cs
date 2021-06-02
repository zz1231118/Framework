using System.Collections.Generic;
using System.Reflection;

namespace Framework.Injection
{
    internal class ServiceMetadata
    {
        public bool IsAutowired;

        public ConstructorInfo Constructor;

        public List<ServiceFieldMetadata> Fields;
    }

    internal sealed class ServiceFieldMetadata
    {
        public FieldInfo FieldInfo;

        public Automatic Options;

        public void SetValue(object obj, object value)
        {
            FieldInfo.SetValue(obj, value);
        }
    }
}
