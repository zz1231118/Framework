using System.Collections.Generic;
using System.Linq;
using SimpleProtoGenerater.Generater.Statements;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class ImportsDefinition : NodeTree
    {
        public ImportsDefinition(IReadOnlyList<INode> nodes)
            : base(nodes)
        { }

        public IReadOnlyCollection<ImportDefinition> Imports => Nodes.OfType<ImportDefinition>().ToList();
    }
}
