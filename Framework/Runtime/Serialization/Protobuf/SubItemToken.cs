namespace Framework.Runtime.Serialization.Protobuf
{
    public readonly ref struct SubItemToken
    {
        internal readonly uint depth;
        internal readonly uint value;
        internal readonly WireType wireType;

        internal SubItemToken(uint depth, uint value, WireType wireType)
        {
            this.depth = depth;
            this.value = value;
            this.wireType = wireType;
        }
    }
}
