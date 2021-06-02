using System;
using System.Net.Sockets;

namespace Framework.Net.Sockets
{
    public class DefaultPacketProcessor : IPacketProcessor
    {
        private void HandlePrefix(DataToken dataToken, byte[] buffer, ref int offset, ref int count)
        {
            var differBytes = DataToken.PrefixLength - dataToken.PrefixBytesDone;
            var copyedBytes = count >= differBytes ? differBytes : count;
            Buffer.BlockCopy(buffer, offset, dataToken.ByteArrayForPrefix, dataToken.PrefixBytesDone, copyedBytes);

            offset += copyedBytes;
            count -= copyedBytes;
            dataToken.PrefixBytesDone += copyedBytes;

            if (dataToken.IsPrefixReady)
                dataToken.MessageLength = BitConverter.ToInt32(dataToken.ByteArrayForPrefix, 0);
        }

        private void HandleBody(DataToken dataToken, byte[] buffer, ref int offset, ref int count)
        {
            if (dataToken.MessageBytesDone == 0)
                dataToken.ByteArrayForMessage = new byte[dataToken.MessageLength];

            var differBytes = dataToken.MessageLength - dataToken.MessageBytesDone;
            var copyedBytes = count >= differBytes ? differBytes : count;
            Buffer.BlockCopy(buffer, offset, dataToken.ByteArrayForMessage, dataToken.MessageBytesDone, copyedBytes);

            offset += copyedBytes;
            count -= copyedBytes;
            dataToken.MessageBytesDone += copyedBytes;
        }

        public bool HandlePacket(SocketAsyncEventArgs ioEventArgs, PacketStreamer streamer)
        {
            var dataToken = (DataToken)ioEventArgs.UserToken;
            var offset = ioEventArgs.Offset;
            var count = ioEventArgs.BytesTransferred;

            do
            {
                if (!dataToken.IsPrefixReady)
                {
                    HandlePrefix(dataToken, ioEventArgs.Buffer, ref offset, ref count);
                    if (dataToken.IsPrefixReady && (dataToken.MessageLength > DataToken.MaxMessageLength || dataToken.MessageLength <= 0))
                        return false;
                    if (count == 0)
                        break;
                }

                HandleBody(dataToken, ioEventArgs.Buffer, ref offset, ref count);
                if (dataToken.IsMessageReady)
                {
                    streamer.Enqueue(dataToken.ByteArrayForMessage);
                    dataToken.Reset();
                }
            } while (count > 0);
            return true;
        }

        public byte[] BuildPacket(byte[] data, int offset, int count)
        {
            var buffer = new byte[count + DataToken.PrefixLength];
            var byteArrayForLength = BitConverter.GetBytes(data.Length);
            Buffer.BlockCopy(byteArrayForLength, 0, buffer, 0, DataToken.PrefixLength);
            Buffer.BlockCopy(data, offset, buffer, DataToken.PrefixLength, count);
            return buffer;
        }
    }
}