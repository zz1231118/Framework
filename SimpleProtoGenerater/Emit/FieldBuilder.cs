using System.Text;

namespace SimpleProtoGenerater.Emit
{
    public class FieldBuilder : MemberBuilder
    {
        private readonly object _defaultValue;

        internal FieldBuilder(AssemblyBuilder assembly, string name, object defaultValue)
            : base(assembly, name)
        {
            _defaultValue = defaultValue;
        }

        public object DefaultValue => _defaultValue;

        public override void BuildCode(StringBuilder sb, int depth)
        {
            var tab = new string(' ', depth * 4);
            sb.Append(tab).Append(Name).Append(" = ").Append(_defaultValue).AppendLine(",");
        }
    }
}
