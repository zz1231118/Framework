using System.Collections.Generic;
using System.Linq;
using SimpleProtoGenerater.Generater.Statements;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class NamespaceDefinition : NodeTree
    {
        public NamespaceDefinition(IReadOnlyList<INode> nodes)
            : base(nodes)
        { }

        public string Namespace => string.Join(".", Nodes.Cast<NodeLeaf>().Select(p => p.Token.Image));
    }
}
