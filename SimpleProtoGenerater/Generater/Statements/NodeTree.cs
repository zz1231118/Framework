using System.Collections.Generic;

namespace SimpleProtoGenerater.Generater.Statements
{
    public class NodeTree : INode
    {
        private readonly IReadOnlyList<INode> _nodes;

        public NodeTree(IReadOnlyList<INode> nodes)
        {
            _nodes = nodes;
        }

        public IReadOnlyList<INode> Nodes => _nodes;
    }
}
