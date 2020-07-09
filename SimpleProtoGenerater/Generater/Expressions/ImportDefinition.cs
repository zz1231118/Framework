using System.Collections.Generic;
using System.Linq;
using SimpleProtoGenerater.Generater.Statements;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class ImportDefinition : NodeTree
    {
        public ImportDefinition(IReadOnlyList<INode> nodes)
            : base(nodes)
        { }

        public string Import => string.Join(".", Nodes.Cast<NodeLeaf>().Select(p => p.Token.Image));
    }
}
