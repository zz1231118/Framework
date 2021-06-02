namespace Framework.Runtime.Serialization.Protobuf
{
    public interface IMessage
    {
        void ReadFrom(ProtoReader reader);

        void WriteTo(ProtoWriter writer);
    }
}
