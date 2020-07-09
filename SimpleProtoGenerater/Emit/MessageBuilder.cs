using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleProtoGenerater.Emit
{
    public class MessageBuilder : MemberBuilder
    {
        private readonly List<MemberBuilder> _members = new List<MemberBuilder>();

        internal MessageBuilder(AssemblyBuilder assembly, string baseType, string name)
            : base(assembly, name)
        {
            BaseType = baseType;
        }

        public string BaseType { get; }
        public string Namespace => Assembly.Namespace;
        public IReadOnlyList<PropertyBuilder> Properties => new List<PropertyBuilder>(_members.OfType<PropertyBuilder>());
        public IReadOnlyList<MessageBuilder> Types => new List<MessageBuilder>(_members.OfType<MessageBuilder>());
        public IReadOnlyList<EnumBuilder> Enums => new List<EnumBuilder>(_members.OfType<EnumBuilder>());

        internal void AddMember(MemberBuilder member)
        {
            _members.Add(member);
        }
        internal MessageBuilder GetType(string name)
        {
            return _members.OfType<MessageBuilder>().FirstOrDefault(p => p.Name == name) ?? Assembly.GetType(name) ?? SystemAssembly.GetType(name);
        }
        internal EnumBuilder GetEnum(string name)
        {
            return _members.OfType<EnumBuilder>().FirstOrDefault(p => p.Name == name) ?? Assembly.GetEnum(name);
        }

        public override void BuildCode(StringBuilder sb, int depth)
        {
            foreach (var property in Properties)
            {
                if (property.Field <= 0)
                    throw new InvalidOperationException(string.Format("Type:{0}.{1} field:{2} index error!", Name, property.Name, property.Field));
            }

            var tab = new string(' ', (depth + 0) * 4);
            var mtab = new string(' ', (depth + 1) * 4);
            var mftab = new string(' ', (depth + 2) * 4);
            var mfmtab = new string(' ', (depth + 3) * 4);
            var mfmstab = new string(' ', (depth + 4) * 4);
            var mfmsstab = new string(' ', (depth + 5) * 4);
            var mfmssftab = new string(' ', (depth + 6) * 4);

            sb.Append(tab).AppendLine("/// <summary>");
            sb.Append(tab).AppendLine("/// <para>自动生成代码 请勿修改</para>");
            sb.Append(tab).AppendLine("/// </summary>");
            sb.Append(tab).Append("public partial class ").Append(Name).Append(" : ").AppendLine(BaseType ?? "ProtoObject");
            sb.Append(tab).AppendLine("{");
            foreach (var property in Properties)
            {
                sb.Append(mtab).Append("private ").Append(GetPropertyType(property)).Append(" _").Append(GetFieldName(property.Name)).AppendLine(";");
            }

            if (Properties.Count > 0)
            {
                sb.AppendLine();
            }
            foreach (var property in Properties)
            {
                var name = GetFieldName(property.Name);
                sb.Append(mtab).Append("public ").Append(GetPropertyType(property)).Append(" ").AppendLine(GetPropertyName(property));
                sb.Append(mtab).AppendLine("{");
                if (property.Mode == PropertyMode.Repeated)
                {
                    sb.Append(mftab).Append("get => _").Append(name).Append(" ?? (_").Append(name).Append(" = new List<").Append(property.PropertyType).AppendLine(">());");
                }
                else
                {
                    sb.Append(mftab).Append("get => _").Append(name).AppendLine(";");
                    sb.Append(mftab).AppendLine("set");
                    sb.Append(mftab).AppendLine("{");
                    sb.Append(mfmtab).Append("_").Append(name).AppendLine(" = value;");
                    sb.Append(mfmtab).Append("SetFieldFlag(").Append(property.Field).AppendLine(");");
                    sb.Append(mftab).AppendLine("}");
                }

                sb.Append(mtab).AppendLine("}");
            }

            if (Properties.Count > 0)
            {
                sb.AppendLine();
            }
            sb.Append(mtab).AppendLine("public override void WriteTo(ProtoWriter writer)");
            sb.Append(mtab).AppendLine("{");
            if (BaseType != null)
            {
                sb.Append(mftab).Append("writer.WriteFieldHeader(0, WireType.StartGroup);").AppendLine();
                sb.Append(mftab).Append("var tokenBase = writer.StartSubItem(this);").AppendLine();
                sb.Append(mftab).Append("base.WriteTo(writer);").AppendLine();
                sb.Append(mftab).Append("writer.EndSubItem(tokenBase);").AppendLine();
            }
            foreach (var property in Properties)
            {
                var name = GetFieldName(property.Name);
                if (property.Mode == PropertyMode.Repeated)
                {
                    sb.Append(mftab).Append("if (_").Append(name).Append(" != null && _").Append(name).AppendLine(".Count > 0)");
                    sb.Append(mftab).AppendLine("{");
                    sb.Append(mfmtab).Append("for (int i = 0; i < _").Append(name).AppendLine(".Count; i++)");
                    sb.Append(mfmtab).AppendLine("{");
                    if (TryWriteProperty(property, "_" + name + "[i]", out string value))
                    {
                        sb.Append(mfmstab).Append("writer.WriteFieldHeader(").Append(property.Field).Append(", ").Append(GetWireType(property)).AppendLine(");");
                        sb.Append(mfmstab).Append(value).AppendLine(";");
                    }
                    else
                    {
                        sb.Append(mfmstab).Append("writer.WriteFieldHeader(").Append(property.Field).Append(", ").Append(GetWireType(property)).AppendLine(");");
                        sb.Append(mfmstab).Append("var ").Append(name).Append("Token = writer.StartSubItem(_").Append(name).AppendLine("[i]);");
                        sb.Append(mfmstab).Append("_").Append(name).AppendLine("[i].WriteTo(writer);");
                        sb.Append(mfmstab).Append("writer.EndSubItem(").Append(name).AppendLine("Token);");
                    }

                    sb.Append(mfmtab).AppendLine("}");
                    sb.Append(mftab).AppendLine("}");
                }
                else
                {
                    if (property.PropertyType.ToLower() == "string" || (property.PropertyType.ToLower() == "byte" && property.IsArray))
                    {
                        sb.Append(mftab).Append("if (_").Append(name).AppendLine(" != null)");
                        sb.Append(mftab).AppendLine("{");
                        sb.Append(mfmtab).Append("writer.WriteFieldHeader(").Append(property.Field).Append(", ").Append(GetWireType(property)).AppendLine(");");
                        TryWriteProperty(property, "_" + name, out string value);
                        sb.Append(mfmtab).Append(value).AppendLine(";");
                        sb.Append(mftab).AppendLine("}");
                    }
                    else if (TryWriteProperty(property, "_" + name, out string value))
                    {
                        sb.Append(mftab).Append("writer.WriteFieldHeader(").Append(property.Field).Append(", ").Append(GetWireType(property)).AppendLine(");");
                        sb.Append(mftab).Append(value).AppendLine(";");
                    }
                    else
                    {
                        sb.Append(mftab).Append("if (_").Append(name).AppendLine(" != null)");
                        sb.Append(mftab).AppendLine("{");
                        sb.Append(mfmtab).Append("writer.WriteFieldHeader(").Append(property.Field).Append(", ").Append(GetWireType(property)).AppendLine(");");
                        sb.Append(mfmtab).Append("var ").Append(name).Append("Token = writer.StartSubItem(_").Append(name).AppendLine(");");
                        sb.Append(mfmtab).Append("_").Append(name).AppendLine(".WriteTo(writer);");
                        sb.Append(mfmtab).Append("writer.EndSubItem(").Append(name).AppendLine("Token);");
                        sb.Append(mftab).AppendLine("}");
                    }
                }
            }

            sb.Append(mtab).AppendLine("}");
            sb.Append(mtab).AppendLine("public override void ReadFrom(ProtoReader reader)");
            sb.Append(mtab).AppendLine("{");
            sb.Append(mftab).AppendLine("uint field;");
            sb.Append(mftab).AppendLine("while (reader.TryReadFieldHeader(out field))");
            sb.Append(mftab).AppendLine("{");
            sb.Append(mfmtab).AppendLine("switch (field)");
            sb.Append(mfmtab).AppendLine("{");
            if (BaseType != null)
            {
                sb.Append(mfmstab).AppendLine("case 0:");
                sb.Append(mfmsstab).Append("var ").AppendLine("baseToken = reader.StartSubItem();");
                sb.Append(mfmsstab).AppendLine("base.ReadFrom(reader);");
                sb.Append(mfmsstab).Append("reader.EndSubItem(").AppendLine("baseToken);");
                sb.Append(mfmsstab).AppendLine("break;");
            }
            foreach (var property in Properties)
            {
                var name = GetFieldName(property.Name);
                sb.Append(mfmstab).Append("case ").Append(property.Field).AppendLine(":");
                if (property.Mode == PropertyMode.Repeated)
                {
                    if (TryReadProperty(property, out string value))
                    {
                        sb.Append(mfmsstab).Append("_").Append(name).Append(".Add(").Append(value).AppendLine(");");
                    }
                    else
                    {
                        var ptn = GetFieldName(property.PropertyType);
                        sb.Append(mfmsstab).Append("var ").Append(name).AppendLine("Token = reader.StartSubItem();");
                        sb.Append(mfmsstab).Append("var ").Append(ptn).Append(" = new ").Append(property.PropertyType).AppendLine("();");
                        sb.Append(mfmsstab).Append(ptn).AppendLine(".ReadFrom(reader);");
                        sb.Append(mfmsstab).Append("reader.EndSubItem(").Append(name).AppendLine("Token);");
                        sb.Append(mfmsstab).Append(GetPropertyName(property)).Append(".Add(").Append(ptn).AppendLine(");");
                    }
                }
                else
                {
                    if (TryReadProperty(property, out string value))
                    {
                        sb.Append(mfmsstab).Append("_").Append(name).Append(" = ").Append(value).AppendLine(";");
                    }
                    else
                    {
                        sb.Append(mfmsstab).Append("var ").Append(name).AppendLine("Token = reader.StartSubItem();");
                        sb.Append(mfmsstab).Append("_").Append(name).Append(" = new ").Append(property.PropertyType).AppendLine("();");
                        sb.Append(mfmsstab).Append("_").Append(name).AppendLine(".ReadFrom(reader);");
                        sb.Append(mfmsstab).Append("reader.EndSubItem(").Append(name).AppendLine("Token);");
                    }
                }
                sb.Append(mfmsstab).AppendLine("break;");
            }
            sb.Append(mfmstab).AppendLine("default:");
            sb.Append(mfmsstab).AppendLine("reader.SkipField();");
            sb.Append(mfmsstab).AppendLine("break;");
            sb.Append(mfmtab).AppendLine("}");
            sb.Append(mftab).AppendLine("}");
            sb.Append(mtab).AppendLine("}");
            if (_members.Count(p => p is MessageBuilder || p is EnumBuilder) > 0)
                sb.AppendLine();

            foreach (var member in _members)
            {
                if (member is MessageBuilder tb)
                {
                    tb.BuildCode(sb, depth + 1);
                }
                else if (member is EnumBuilder eb)
                {
                    eb.BuildCode(sb, depth + 1);
                }
            }
            sb.Append(tab).AppendLine("}");
        }

        private string GetFieldName(string name)
        {
            var array = name.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                if (char.IsLetter(array[i]))
                {
                    if (char.IsUpper(array[i]))
                    {
                        array[i] = char.ToLower(array[i]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return new string(array);
        }
        private string GetPropertyName(PropertyBuilder property)
        {
            return property.Name.Substring(0, 1).ToUpper() + property.Name.Substring(1);
        }
        private string GetPropertyType(PropertyBuilder property)
        {
            var type = property.IsArray ? property.PropertyType + "[]" : property.PropertyType;
            return property.Mode == PropertyMode.Repeated ? "List<" + type + ">" : type;
        }
        private string GetWireType(PropertyBuilder property)
        {
            var enumType = GetEnum(property.PropertyType);
            if (enumType != null)
            {
                return "WireType.Variant";
            }

            switch (property.PropertyType)
            {
                case "byte":
                    return property.IsArray ? "WireType.Binary" : "WireType.Variant";
                case "sbyte":
                case "short":
                case "ushort":
                case "int":
                case "uint":
                case "long":
                case "ulong":
                case "float":
                case "double":
                case "char":
                case "bool":
                    return "WireType.Variant";
                case "string":
                    return "WireType.String";
                default:
                    return "WireType.StartGroup";
            }
        }
        private bool TryWriteProperty(PropertyBuilder property, string name, out string value)
        {
            var enumType = GetEnum(property.PropertyType);
            if (enumType != null)
            {
                value = "writer.WriteEnum(" + name + ")";
                return true;
            }

            switch (property.PropertyType)
            {
                case "byte":
                    value = property.IsArray ? "writer.WriteBytes(" + name + ")" : "writer.WriteByte(" + name + ")";
                    return true;
                case "sbyte":
                    value = "writer.WriteSByte(" + name + ")";
                    return true;
                case "short":
                    value = "writer.WriteInt16(" + name + ")";
                    return true;
                case "ushort":
                    value = "writer.WriteUInt16(" + name + ")";
                    return true;
                case "int":
                    value = "writer.WriteInt32(" + name + ")";
                    return true;
                case "uint":
                    value = "writer.WriteUInt32(" + name + ")";
                    return true;
                case "long":
                    value = "writer.WriteInt64(" + name + ")";
                    return true;
                case "ulong":
                    value = "writer.WriteUInt64(" + name + ")";
                    return true;
                case "float":
                    value = "writer.WriteSingle(" + name + ")";
                    return true;
                case "double":
                    value = "writer.WriteDouble(" + name + ")";
                    return true;
                case "char":
                    value = "writer.WriteChar(" + name + ")";
                    return true;
                case "bool":
                    value = "writer.WriteBoolean(" + name + ")";
                    return true;
                case "string":
                    value = "writer.WriteString(" + name + ")";
                    return true;
                default:
                    value = null;
                    return false;
            }
        }
        private bool TryReadProperty(PropertyBuilder property, out string value)
        {
            var enumType = GetEnum(property.PropertyType);
            if (enumType != null)
            {
                value = "reader.ReadEnum<" + property.PropertyType + ">()";
                return true;
            }

            switch (property.PropertyType)
            {
                case "byte":
                    value = property.IsArray ? "reader.ReadBytes()" : "reader.ReadByte()";
                    return true;
                case "sbyte":
                    value = "reader.ReadSByte()";
                    return true;
                case "short":
                    value = "reader.ReadInt16()";
                    return true;
                case "ushort":
                    value = "reader.ReadUInt16()";
                    return true;
                case "int":
                    value = "reader.ReadInt32()";
                    return true;
                case "uint":
                    value = "reader.ReadUInt32()";
                    return true;
                case "long":
                    value = "reader.ReadInt64()";
                    return true;
                case "ulong":
                    value = "reader.ReadUInt64()";
                    return true;
                case "float":
                    value = "reader.ReadSingle()";
                    return true;
                case "double":
                    value = "reader.ReadDouble()";
                    return true;
                case "char":
                    value = "reader.ReadChar()";
                    return true;
                case "bool":
                    value = "reader.ReadBoolean()";
                    return true;
                case "string":
                    value = "reader.ReadString()";
                    return true;
                default:
                    value = null;
                    return false;
            }
        }
    }
}