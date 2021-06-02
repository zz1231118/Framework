using System;

namespace Framework.Injection
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AutowiredAttribute : Attribute
    {
        private readonly Automatic options;

        public AutowiredAttribute()
        { }

        public AutowiredAttribute(Automatic options)
        {
            this.options = options;
        }

        public Automatic Options => options;
    }

    public enum Automatic : byte
    {
        Optional,
        Required
    }
}
