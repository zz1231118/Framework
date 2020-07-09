using System.Collections.Generic;
using System.Text;

namespace SimpleProtoGenerater.Emit
{
    public class ServiceBuilder : MemberBuilder
    {
        private readonly string[] interfaces;
        private readonly List<MethodBuilder> methods = new List<MethodBuilder>();

        internal ServiceBuilder(AssemblyBuilder assembly, string name, string[] interfaces)
            : base(assembly, name)
        {
            this.interfaces = interfaces;
        }

        public void AddMethod(MethodBuilder method)
        {
            methods.Add(method);
        }

        public override void BuildCode(StringBuilder sb, int depth)
        {
            var tab = new string(' ', (depth + 0) * 4);
            sb.Append(tab).AppendLine("/// <summary>");
            sb.Append(tab).AppendLine("/// <para>自动生成代码 请勿修改</para>");
            sb.Append(tab).AppendLine("/// </summary>");
            sb.Append(tab).Append("public interface ").Append(Name);
            var e = interfaces.GetEnumerator();
            if (e.MoveNext())
            {
                sb.Append(" : ");
                sb.Append(e.Current);
                while (e.MoveNext())
                {
                    sb.Append(", ");
                    sb.Append(e.Current);
                }
            }
            sb.AppendLine();
            sb.Append(tab).AppendLine("{");
            if (methods.Count > 0)
            {
                methods[0].BuildCode(sb, depth + 1);
                sb.AppendLine();
                for (int i = 1; i < methods.Count; i++)
                {
                    sb.AppendLine();
                    methods[0].BuildCode(sb, depth + 1);
                    sb.AppendLine();
                }
            }

            sb.Append(tab).AppendLine("}");
        }
    }
}
