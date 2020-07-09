using System;
using System.Collections.Generic;

namespace Framework.Data
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class EntityIndexAttribute : Attribute
    {
        public EntityIndexAttribute(IndexCategory category, bool unique, params string[] columns)
        {
            Category = category;
            Unique = unique;
            Columns = columns;
        }

        public IndexCategory Category { get; }

        public bool Unique { get; }

        public IReadOnlyList<string> Columns { get; }
    }
}
