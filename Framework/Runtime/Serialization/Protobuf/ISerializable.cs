namespace Framework.Runtime.Serialization.Protobuf
{
    public interface ISerializable
    {
        void ReadFrom(ProtoReader reader);

        void WriteTo(ProtoWriter writer);
    }
}
