using System.Collections.Generic;
using System.Linq;
using SimpleProtoGenerater.Emit;
using SimpleProtoGenerater.Generater.Statements;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class ServiceDefinition : MemberDefinition
    {
        public ServiceDefinition(IReadOnlyList<INode> nodes)
            : base(nodes)
        { }

        public IReadOnlyCollection<TypeDefinition> Interfaces => Nodes.OfType<TypeDefinition>().ToList();

        public IReadOnlyList<MethodDefinition> Methods => new List<MethodDefinition>(Nodes.OfType<MethodDefinition>());

        public ServiceBuilder Build(AssemblyBuilder assembly)
        {
            var eb = new ServiceBuilder(assembly, Name, Interfaces.Select(p => p.Type).ToArray());
            foreach (var method in Methods)
            {
                eb.AddMethod(method.Build(assembly));
            }
            return eb;
        }
    }
}
