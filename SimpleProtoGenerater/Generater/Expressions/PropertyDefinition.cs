using System;
using System.Collections.Generic;
using System.Linq;
using SimpleProtoGenerater.Emit;
using SimpleProtoGenerater.Generater.Statements;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class PropertyDefinition : MemberDefinition
    {
        public PropertyDefinition(IReadOnlyList<INode> nodes)
            : base(nodes)
        { }

        public string Mode => ((NodeLeaf)Nodes[0]).Token.Image;

        public string PropertyType => ((NodeLeaf)Nodes[1]).Token.Image;

        public int Field => (int)Nodes.OfType<NumberDefinition>().First().Value;

        public bool IsArray => Nodes.Any(p => p is ArrayDefinition);

        public PropertyBuilder Build(AssemblyBuilder assembly)
        {
            var mode = (PropertyMode)Enum.Parse(typeof(PropertyMode), Mode, true);
            return new PropertyBuilder(assembly, Name, mode, PropertyType, Field, IsArray);
        }

        public override string ToString()
        {
            return string.Format("public {0} {1} {{ get; set; }}", PropertyType, Name);
        }
    }
}
