using System.Collections.Generic;
using System.Linq;
using SimpleProtoGenerater.Emit;
using SimpleProtoGenerater.Generater.Statements;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class AssemblyDefinition : NodeTree
    {
        public AssemblyDefinition(IReadOnlyList<INode> nodes)
            : base(nodes)
        { }

        public IReadOnlyList<MessageDefinition> Types => new List<MessageDefinition>(Nodes.OfType<MessageDefinition>());
        public IReadOnlyList<EnumDefinition> Enums => new List<EnumDefinition>(Nodes.OfType<EnumDefinition>());

        public AssemblyBuilder Build(IEnumerable<string> imports, string @namespace)
        {
            var assembly = new AssemblyBuilder(imports, @namespace);
            foreach (var node in Nodes)
            {
                switch (node)
                {
                    case MessageDefinition other:
                        assembly.AddMember(other.Build(assembly));
                        break;
                    case EnumDefinition other:
                        assembly.AddMember(other.Build(assembly));
                        break;
                    case ServiceDefinition other:
                        assembly.AddMember(other.Build(assembly));
                        break;
                }
            }
            return assembly;
        }
    }
}
