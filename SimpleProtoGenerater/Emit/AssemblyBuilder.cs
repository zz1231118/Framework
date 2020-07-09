using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleProtoGenerater.Emit
{
    public class AssemblyBuilder
    {
        private readonly string _namespace;
        private readonly List<string> _references = new List<string>();
        private readonly List<MemberBuilder> _members = new List<MemberBuilder>();

        internal AssemblyBuilder(IEnumerable<string> imports, string @namespace)
        {
            _namespace = @namespace;

            _references.Add("System");
            _references.Add("System.Collections.Generic");
            if (imports != null)
            {
                foreach (var import in imports)
                    _references.Add(import);
            }
        }

        public string Namespace => _namespace;
        public IReadOnlyList<string> References => _references;
        public IReadOnlyList<MessageBuilder> Types => new List<MessageBuilder>(_members.OfType<MessageBuilder>());
        public IReadOnlyList<EnumBuilder> Enums => new List<EnumBuilder>(_members.OfType<EnumBuilder>());

        internal void AddMember(MemberBuilder member)
        {
            _members.Add(member);
        }
        internal MessageBuilder GetType(string name)
        {
            return _members.OfType<MessageBuilder>().FirstOrDefault(p => p.Name == name);
        }
        internal EnumBuilder GetEnum(string name)
        {
            return _members.OfType<EnumBuilder>().FirstOrDefault(p => p.Name == name);
        }

        public void AddReference(string reference)
        {
            if (!_references.Contains(reference))
            {
                _references.Add(reference);
            }
        }
        public void BuildCode(StringBuilder sb, int depth = 0)
        {
            var tab = new string(' ', depth * 4);
            foreach (var @ref in _references)
            {
                sb.Append(tab).Append("using ").Append(@ref).AppendLine(";");
            }

            sb.AppendLine();
            if (_namespace != null)
            {
                sb.Append(tab).Append("namespace ").AppendLine(_namespace);
                sb.Append(tab).AppendLine("{");
                depth++;
            }

            var mtab = new string(' ', (depth) * 4);
            var mftab = new string(' ', (depth + 1) * 4);
            var m2ftab = new string(' ', (depth + 2) * 4);
            var m3ftab = new string(' ', (depth + 3) * 4);
            sb.Append(mtab).AppendLine("/// <summary>");
            sb.Append(mtab).AppendLine("/// <para>自动生成代码 请勿修改</para>");
            sb.Append(mtab).AppendLine("/// </summary>");
            sb.Append(mtab).AppendLine("public abstract class ProtoObject : ISerializable");
            sb.Append(mtab).AppendLine("{");

            sb.Append(mftab).AppendLine("private static readonly int[] DefaultFlags = new int[0];");
            sb.Append(mftab).AppendLine("private int[] flags = DefaultFlags;");
            sb.AppendLine();

            sb.Append(mftab).AppendLine("public abstract void WriteTo(ProtoWriter writer);");
            sb.AppendLine();

            sb.Append(mftab).AppendLine("public abstract void ReadFrom(ProtoReader reader);");
            sb.AppendLine();

            sb.Append(mftab).AppendLine("public bool HasFieldFlag(ushort field)");
            sb.Append(mftab).AppendLine("{");
            sb.Append(m2ftab).AppendLine("var index = field - 1;");
            sb.Append(m2ftab).AppendLine("var unitSize = sizeof(int) * 8;");
            sb.Append(m2ftab).AppendLine("return flags.Length > index ? (flags[index / unitSize] & (1 << (index % unitSize))) != 0 : false;");
            sb.Append(mftab).AppendLine("}");
            sb.AppendLine();

            sb.Append(mftab).AppendLine("protected void SetFieldFlag(ushort field)");
            sb.Append(mftab).AppendLine("{");
            sb.Append(m2ftab).AppendLine("var index = field - 1;");
            sb.Append(m2ftab).AppendLine("var unitSize = sizeof(int) * 8;");
            sb.Append(m2ftab).AppendLine("if (flags.Length <= index)");
            sb.Append(m2ftab).AppendLine("{");
            sb.Append(m3ftab).AppendLine("var newFlags = new int[index + 1];");
            sb.Append(m3ftab).AppendLine("if (flags.Length > 0) Array.Copy(flags, 0, newFlags, 0, flags.Length);");
            sb.Append(m3ftab).AppendLine("flags = newFlags;");
            sb.Append(m2ftab).AppendLine("}");
            sb.Append(m2ftab).AppendLine("flags[index / unitSize] |= (1 << (index % unitSize));");
            sb.Append(mftab).AppendLine("}");

            sb.Append(mtab).AppendLine("}");
            foreach (var member in _members)
            {
                member.BuildCode(sb, depth);
            }

            if (_namespace != null)
            {
                sb.Append(tab).AppendLine("}");
            }
        }
    }
}
