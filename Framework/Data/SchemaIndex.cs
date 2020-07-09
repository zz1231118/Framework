using System.Collections.Generic;

namespace Framework.Data
{
    public class SchemaIndex
    {
        public string Name { get; internal set; }

        public IndexCategory Category { get; internal set; }

        public bool Unique { get; internal set; }

        public IReadOnlyList<ISchemaColumn> Columns { get; internal set; }
    }
}
