using System.Collections.Generic;
using System.Text;

namespace SimpleProtoGenerater.Emit
{
    public class EnumBuilder : MemberBuilder
    {
        private readonly string _underlyingSystemType;
        private readonly List<FieldBuilder> _fields = new List<FieldBuilder>();

        public EnumBuilder(AssemblyBuilder assembly, string underlyingSystemType, string name)
            : base(assembly, name)
        {
            _underlyingSystemType = underlyingSystemType;
        }

        public string UnderlyingSystemType => _underlyingSystemType ?? "int";
        public IReadOnlyList<FieldBuilder> Fields => _fields;

        internal void AddField(FieldBuilder field)
        {
            _fields.Add(field);
        }
        internal FieldBuilder DefineLiteral(string literalName, object literalValue)
        {
            var field = new FieldBuilder(Assembly, literalName, literalValue);
            _fields.Add(field);
            return field;
        }

        public override void BuildCode(StringBuilder sb, int depth)
        {
            var tab = new string(' ', depth * 4);
            sb.Append(tab).AppendLine("/// <summary>");
            sb.Append(tab).AppendLine("/// <para>自动生成代码 请勿修改</para>");
            sb.Append(tab).AppendLine("/// </summary>");
            sb.Append(tab).Append("public enum ").Append(Name);
            if (_underlyingSystemType != null)
            {
                sb.Append(" : ").Append(_underlyingSystemType);
            }
            sb.AppendLine();
            sb.Append(tab).AppendLine("{");
            foreach (var field in _fields)
            {
                field.BuildCode(sb, depth + 1);
            }
            sb.Append(tab).AppendLine("}");
        }
    }
}