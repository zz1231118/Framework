using System.Text;

namespace SimpleProtoGenerater.Emit
{
    public abstract class MemberBuilder
    {
        private readonly AssemblyBuilder _assembly;
        private readonly MessageBuilder _declaringType;
        private readonly string _name;

        protected MemberBuilder(AssemblyBuilder assembly, string name)
        {
            _assembly = assembly;
            _name = name;
        }

        public AssemblyBuilder Assembly => _assembly;
        public MessageBuilder DeclaringType => _declaringType;
        public string Name => _name;

        public abstract void BuildCode(StringBuilder sb, int depth);
    }
}
