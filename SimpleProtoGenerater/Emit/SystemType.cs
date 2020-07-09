namespace SimpleProtoGenerater.Emit
{
    class SystemType : MessageBuilder
    {
        private readonly string _struct;

        public SystemType(AssemblyBuilder assembly, string baseType, string name, string @struct)
            : base(assembly, baseType, name)
        {
            _struct = @struct;
        }

        public string Struct => _struct;
    }
}
