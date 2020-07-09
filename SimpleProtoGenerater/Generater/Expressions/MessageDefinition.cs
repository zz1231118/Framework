using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleProtoGenerater.Emit;
using SimpleProtoGenerater.Generater.Statements;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class MessageDefinition : MemberDefinition
    {
        public MessageDefinition(IReadOnlyList<INode> nodes)
            : base(nodes)
        { }

        public string BaseType => Nodes.OfType<TypeDefinition>().FirstOrDefault()?.Type;
        public IReadOnlyList<PropertyDefinition> Properties => new List<PropertyDefinition>(Nodes.OfType<PropertyDefinition>());
        public IReadOnlyList<MessageDefinition> Types => new List<MessageDefinition>(Nodes.OfType<MessageDefinition>());
        public IReadOnlyList<EnumDefinition> Enums => new List<EnumDefinition>(Nodes.OfType<EnumDefinition>());

        public MessageBuilder Build(AssemblyBuilder assembly)
        {
            var tb = new MessageBuilder(assembly, BaseType, Name);
            foreach (var node in Nodes)
            {
                if (node is PropertyDefinition pd)
                {
                    tb.AddMember(pd.Build(assembly));
                }
                else if (node is MessageDefinition td)
                {
                    tb.AddMember(td.Build(assembly));
                }
                else if (node is EnumDefinition ed)
                {
                    tb.AddMember(ed.Build(assembly));
                }
            }
            return tb;
        }
        public void TypeCheck(AssemblyBuilder assembly, MessageBuilder owner)
        {
            if (BaseType != null)
            {

            }
            foreach (var property in Properties)
            {


            }
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("public class ").AppendLine(Name);
            sb.AppendLine("{");
            foreach (var property in Properties)
            {
                sb.Append("    ").AppendLine(property.ToString());
                sb.AppendLine();
            }
            foreach (var type in Types)
            {
                sb.Append("    ").AppendLine(type.ToString());
                sb.AppendLine();
            }
            foreach (var @enum in Enums)
            {
                sb.Append("    ").AppendLine(@enum.ToString());
                sb.AppendLine();
            }
            sb.Append("}");
            return base.ToString();
        }
    }
}
