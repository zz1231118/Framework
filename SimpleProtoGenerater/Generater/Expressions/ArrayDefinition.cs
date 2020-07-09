using System.Collections.Generic;
using SimpleProtoGenerater.Generater.Statements;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class ArrayDefinition : NodeTree
    {
        public ArrayDefinition(IReadOnlyList<INode> nodes)
            : base(nodes)
        { }
    }
}
