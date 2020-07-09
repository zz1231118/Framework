namespace Framework.Runtime.Serialization.Protobuf
{
    public enum WireType : sbyte
    {
        None = -1,
        Variant = 0,
        Fixed16,
        Fixed32,
        Fixed64,
        String,
        Binary,
        StartGroup,
        EndGroup,
    }
}
