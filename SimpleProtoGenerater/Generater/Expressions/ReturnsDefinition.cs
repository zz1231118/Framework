using System.Collections.Generic;
using System.Linq;
using SimpleProtoGenerater.Generater.Statements;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class ReturnsDefinition : NodeTree
    {
        public ReturnsDefinition(IReadOnlyList<INode> nodes)
            : base(nodes)
        { }

        public IReadOnlyCollection<TypeDefinition> Returns => Nodes.OfType<TypeDefinition>().ToList();
    }
}
