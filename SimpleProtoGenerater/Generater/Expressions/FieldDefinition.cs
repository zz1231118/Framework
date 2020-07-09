using System.Collections.Generic;
using System.Linq;
using SimpleProtoGenerater.Emit;
using SimpleProtoGenerater.Generater.Statements;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class FieldDefinition : MemberDefinition
    {
        public FieldDefinition(IReadOnlyList<INode> nodes)
            : base(nodes)
        { }

        public int Value => (int)Nodes.OfType<NumberDefinition>().First().Value;

        public FieldBuilder Build(AssemblyBuilder assembly)
        {
            return new FieldBuilder(assembly, Name, Value);
        }
    }
}
