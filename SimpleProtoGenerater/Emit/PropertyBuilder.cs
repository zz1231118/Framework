using System.Text;

namespace SimpleProtoGenerater.Emit
{
    public class PropertyBuilder : MemberBuilder
    {
        public PropertyBuilder(AssemblyBuilder assembly, string name, PropertyMode mode, string propertyType, int field, bool isArray)
            : base(assembly, name)
        {
            Mode = mode;
            PropertyType = propertyType;
            Field = field;
            IsArray = isArray;
        }

        public PropertyMode Mode { get; }
        public string PropertyType { get; }
        public int Field { get; }
        public bool IsArray { get; }

        public override void BuildCode(StringBuilder sb, int depth)
        {
            var tab = new string(' ', depth * 4);
            sb.Append(tab).Append("public ");
            switch (Mode)
            {
                case PropertyMode.Optional:
                case PropertyMode.Required:
                    sb.Append(PropertyType);
                    break;
                case PropertyMode.Repeated:
                    sb.Append("List<").Append(PropertyType).Append(">");
                    break;
            }

            sb.Append(" ").Append(Name).AppendLine(" { get; set; }");
        }
    }

    public enum PropertyMode
    {
        Optional,
        Required,
        Repeated
    }
}
