using System.Collections.Generic;
using System.Linq;
using SimpleProtoGenerater.Emit;
using SimpleProtoGenerater.Generater.Statements;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class MethodDefinition : MemberDefinition
    {
        public MethodDefinition(IReadOnlyList<INode> nodes)
            : base(nodes)
        { }

        public IReadOnlyCollection<TypeDefinition> Parameters => Nodes.OfType<ParametersDefinition>().FirstOrDefault()?.Parameters;

        public IReadOnlyCollection<TypeDefinition> Returns => Nodes.OfType<ReturnsDefinition>().FirstOrDefault().Returns;

        public MethodBuilder Build(AssemblyBuilder assembly)
        {
            return new MethodBuilder(assembly, Name, Parameters.Select(p => p.Type).ToArray(), Returns.Select(p => p.Type).ToArray());
        }
    }
}
