using System.Linq;

namespace SimpleProtoGenerater.Emit
{
    class SystemAssembly : AssemblyBuilder
    {
        public static SystemAssembly Default = new SystemAssembly();

        private SystemAssembly()
            : base(null, "System")
        {
            AddMember(new SystemType(this, null, "byte", "Byte"));
            AddMember(new SystemType(this, null, "sbyte", "SByte"));
            AddMember(new SystemType(this, null, "short", "Int16"));
            AddMember(new SystemType(this, null, "ushort", "UInt16"));
            AddMember(new SystemType(this, null, "int", "Int32"));
            AddMember(new SystemType(this, null, "uint", "UInt32"));
            AddMember(new SystemType(this, null, "long", "Int64"));
            AddMember(new SystemType(this, null, "ulong", "UInt64"));
            AddMember(new SystemType(this, null, "float", "Single"));
            AddMember(new SystemType(this, null, "double", "Double"));
            AddMember(new SystemType(this, null, "bool", "Boolean"));
            AddMember(new SystemType(this, null, "char", "Char"));
            AddMember(new SystemType(this, null, "string", "String"));
        }

        public static new SystemType GetType(string name)
        {
            return Default.Types.OfType<SystemType>().FirstOrDefault(p => p.Name == name || p.Struct == name);
        }
    }
}
