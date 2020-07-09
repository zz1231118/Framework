using System.Text;

namespace SimpleProtoGenerater.Emit
{
    public class MethodBuilder : MemberBuilder
    {
        public MethodBuilder(AssemblyBuilder assembly, string name, string[] parameters, string[] returns)
            : base(assembly, name)
        {
            Parameters = parameters;
            Returns = returns;
        }

        public string[] Parameters { get; }

        public string[] Returns { get; }

        public override void BuildCode(StringBuilder sb, int depth)
        {
            var tab = new string(' ', depth * 4);
            sb.Append(tab);
            if (Returns.Length == 0)
            {
                sb.Append("void");
            }
            else if (Returns.Length == 1)
            {
                sb.Append(Returns[0]);
            }
            else
            {
                sb.Append("(").Append(Returns[0]);
                for (int i = 1; i < Returns.Length; i++)
                {
                    sb.Append(", ").Append(Returns[i]);
                }
                sb.Append(")");
            }
            sb.Append(" ").Append(Name).Append("(");
            var e = Parameters.GetEnumerator();
            if (e.MoveNext())
            {
                sb.Append(e.Current).Append(" req");
                while (e.MoveNext())
                {
                    sb.Append(", ").Append(e.Current);
                }
            }
            sb.Append(");");
        }
    }
}
