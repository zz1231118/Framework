using System.Collections.Generic;
using System.Linq;
using SimpleProtoGenerater.Generater.Statements;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class ParametersDefinition : NodeTree
    {
        public ParametersDefinition(IReadOnlyList<INode> nodes)
            : base(nodes)
        { }

        public IReadOnlyCollection<TypeDefinition> Parameters => Nodes.OfType<TypeDefinition>().ToList();
    }
}
