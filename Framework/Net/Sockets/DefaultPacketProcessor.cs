using System;
using System.Net.Sockets;

namespace Framework.Net.Sockets
{
    public class DefaultPacketProcessor : IPacketProcessor
    {
        public bool HandlePacket(SocketAsyncEventArgs ioEventArgs, PacketStreamer streamer)
        {
            var dataToken = (DataToken)ioEventArgs.UserToken;
            int offset = ioEventArgs.Offset;
            int count = ioEventArgs.BytesTransferred;

            do
            {
                if (!dataToken.IsPrefixReady)
                {
                    HandlePrefix(dataToken, ioEventArgs.Buffer, ref offset, ref count);
                    if (dataToken.IsPrefixReady && (dataToken.messageLength > DataToken.MaxMessageLength || dataToken.messageLength <= 0))
                        return false;
                    if (count == 0)
                        break;
                }

                HandleBody(dataToken, ioEventArgs.Buffer, ref offset, ref count);
                if (dataToken.IsMessageReady)
                {
                    streamer.Enqueue(dataToken.byteArrayForMessage);
                    dataToken.Reset();
                }
            }
            while (count > 0);
            return true;
        }

        public byte[] BuildPacket(byte[] data, int offset, int count)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (offset + count > data.Length)
                throw new IndexOutOfRangeException();

            var buffer = new byte[count + DataToken.PrefixLength];
            Buffer.BlockCopy(BitConverter.GetBytes(data.Length), 0, buffer, 0, DataToken.PrefixLength);
            Buffer.BlockCopy(data, offset, buffer, DataToken.PrefixLength, count);
            return buffer;
        }

        private void HandlePrefix(DataToken dataToken, byte[] buffer, ref int offset, ref int count)
        {
            var differBytes = DataToken.PrefixLength - dataToken.prefixBytesDone;
            var copyedBytes = count >= differBytes ? differBytes : count;
            Buffer.BlockCopy(buffer, offset, dataToken.byteArrayForPrefix, dataToken.prefixBytesDone, copyedBytes);

            offset += copyedBytes;
            count -= copyedBytes;
            dataToken.prefixBytesDone += copyedBytes;

            if (dataToken.IsPrefixReady)
                dataToken.messageLength = BitConverter.ToInt32(dataToken.byteArrayForPrefix, 0);
        }

        private void HandleBody(DataToken dataToken, byte[] buffer, ref int offset, ref int count)
        {
            if (dataToken.messageBytesDone == 0)
                dataToken.byteArrayForMessage = new byte[dataToken.messageLength];

            var differBytes = dataToken.messageLength - dataToken.messageBytesDone;
            var copyedBytes = count >= differBytes ? differBytes : count;
            Buffer.BlockCopy(buffer, offset, dataToken.byteArrayForMessage, dataToken.messageBytesDone, copyedBytes);

            offset += copyedBytes;
            count -= copyedBytes;
            dataToken.messageBytesDone += copyedBytes;
        }
    }
}