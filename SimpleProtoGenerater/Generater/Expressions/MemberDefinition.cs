using System.Collections.Generic;
using System.Linq;
using SimpleProtoGenerater.Generater.Statements;

namespace SimpleProtoGenerater.Generater.Expressions
{
    public abstract class MemberDefinition : NodeTree
    {
        public MemberDefinition(IReadOnlyList<INode> nodes)
            : base(nodes)
        { }

        public string Name => Nodes.OfType<AccessDefinition>().First().Name;
    }
}
