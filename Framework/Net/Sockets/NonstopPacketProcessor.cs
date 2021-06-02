using System;
using System.Net.Sockets;

namespace Framework.Net.Sockets
{
    public class NonstopPacketProcessor : IPacketProcessor
    {
        public byte[] BuildPacket(byte[] data, int offset, int count)
        {
            if (offset == 0 && count == data.Length)
            {
                return data;
            }

            var bytes = new byte[count];
            Array.Copy(data, offset, bytes, 0, count);
            return bytes;
        }

        public bool HandlePacket(SocketAsyncEventArgs ioEventArgs, PacketStreamer streamer)
        {
            var length = ioEventArgs.BytesTransferred;
            var bytes = new byte[length];
            Array.Copy(ioEventArgs.Buffer, ioEventArgs.Offset, bytes, 0, length);
            streamer.Enqueue(bytes);
            return true;
        }
    }
}
