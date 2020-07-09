using System.Collections.Generic;
using System.Linq;
using SimpleProtoGenerater.Emit;
using SimpleProtoGenerater.Generater.Statements;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class EnumDefinition : MemberDefinition
    {
        public EnumDefinition(IReadOnlyList<INode> nodes)
            : base(nodes)
        { }

        public string UnderlyingSystemType => Nodes.Count >= 2 && Nodes[1] is NodeLeaf ? ((NodeLeaf)Nodes[1]).Token.Image : null;
        public IReadOnlyList<FieldDefinition> Fields => new List<FieldDefinition>(Nodes.OfType<FieldDefinition>());

        public EnumBuilder Build(AssemblyBuilder assembly)
        {
            var eb = new EnumBuilder(assembly, UnderlyingSystemType, Name);
            foreach (var field in Fields)
            {
                eb.AddField(field.Build(assembly));
            }
            return eb;
        }
    }
}
