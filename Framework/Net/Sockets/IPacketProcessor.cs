using System.Net.Sockets;

namespace Framework.Net.Sockets
{
    public interface IPacketProcessor
    {
        bool HandlePacket(SocketAsyncEventArgs ioEventArgs, PacketStreamer streamer);

        byte[] BuildPacket(byte[] data, int offset, int count);
    }
}