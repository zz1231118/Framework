using System;
using System.Collections.Generic;
using Framework.Net.Sockets;

namespace Framework.Net.WebSockets
{
    class Hybi00MessageProcessor : MessageProcessor
    {
        private const int PreByteLength = 1;
        private const byte StartByte = 0x00;
        private const byte EndByte = 0xFF;
        private static readonly byte[] ClosingBytes = new byte[] { EndByte, StartByte };

        public Hybi00MessageProcessor()
        {
            CloseStatusCode = new Hybi10CloseStatusCode();
        }

        public override bool TryReadMeaage(WSDataToken dataToken, byte[] buffer, out List<DataMessage> messageList)
        {
            messageList = new List<DataMessage>();
            int offset = 0;
            do
            {
                //check close flag
                if (CheckCloseFlag(buffer, ref offset))
                {
                    messageList.Add(new DataMessage() { Data = buffer, Opcode = Opcode.Close });
                    dataToken.Reset();
                    return true;
                }
                //receive buffer is complated
                if (!CheckPrefixHeadComplated(dataToken, buffer, ref offset) ||
                    !CheckDataComplated(dataToken, buffer, ref offset))
                {
                    return false;
                }
                byte[] data = dataToken.byteArrayForMessage;
                if (data != null)
                {
                    messageList.Add(new DataMessage() { Data = data, Opcode = Opcode.Text });
                }
                dataToken.Reset();
            } while (offset < buffer.Length);
            return true;
        }

        private bool CheckCloseFlag(byte[] buffer, ref int offset)
        {
            if (buffer.Length - offset == ClosingBytes.Length &&
                MathUtils.IndexOf(buffer, offset, buffer.Length, ClosingBytes) != -1)
            {
                offset += ClosingBytes.Length;
                return true;
            }
            return false;
        }

        public override byte[] BuildMessagePack(WebSocket exSocket, sbyte opCode, byte[] data, int offset, int count)
        {
            if (opCode == Opcode.Close)
            {
                return ClosingBytes;
            }
            byte[] buffer = new byte[count + 2];
            buffer[0] = StartByte;
            Buffer.BlockCopy(data, offset, buffer, 1, count);
            buffer[count + 1] = EndByte;
            return buffer;
        }

        public override byte[] CloseMessage(WebSocket exSocket, sbyte opCode, string reason)
        {
            return ClosingBytes;
        }

        protected override bool IsValidCloseCode(int code)
        {
            return false;
        }

        private bool CheckPrefixHeadComplated(WSDataToken dataToken, byte[] buffer, ref int offset)
        {
            if (dataToken.byteArrayForPrefix == null || dataToken.byteArrayForPrefix.Length != PreByteLength)
            {
                dataToken.byteArrayForPrefix = new byte[PreByteLength];
            }
            if (PreByteLength - dataToken.prefixBytesDone > buffer.Length - offset)
            {
                Buffer.BlockCopy(buffer, offset, dataToken.byteArrayForPrefix, dataToken.prefixBytesDone, buffer.Length - offset);
                dataToken.prefixBytesDone += buffer.Length - offset;
                return false;
            }

            int count = dataToken.byteArrayForPrefix.Length - dataToken.prefixBytesDone;
            if (count > 0)
            {
                Buffer.BlockCopy(buffer, offset, dataToken.byteArrayForPrefix, dataToken.prefixBytesDone, count);
                dataToken.prefixBytesDone += count;
                offset += count;
            }
            return true;
        }

        private byte[] MergeBytes(params byte[][] args)
        {
            int length = 0;
            foreach (byte[] tempbyte in args)
            {
                length += tempbyte != null ? tempbyte.Length : 0;
            }

            byte[] bytes = new byte[length];

            int tempLength = 0;
            foreach (byte[] tempByte in args)
            {
                if (tempByte == null) continue;
                tempByte.CopyTo(bytes, tempLength);
                tempLength += tempByte.Length;
            }

            return bytes;
        }

        private bool CheckDataComplated(DataToken dataToken, byte[] buffer, ref int offset)
        {
            byte[] data;
            int endMaskIndex = MathUtils.IndexOf(buffer, offset, buffer.Length - offset + 1, new[] { EndByte });
            if (endMaskIndex < 0)
            {
                data = new byte[buffer.Length - offset];
                Buffer.BlockCopy(buffer, offset, data, 0, data.Length);
                if (dataToken.byteArrayForMessage == null)
                {
                    dataToken.byteArrayForMessage = data;
                }
                else
                {
                    dataToken.byteArrayForMessage = MergeBytes(dataToken.byteArrayForMessage, data);
                }
                offset += data.Length;
                return false;
            }
            //end mask not received
            if (endMaskIndex == 0)
            {
                offset += 1;
                return true;
            }
            data = new byte[endMaskIndex - offset];
            Buffer.BlockCopy(buffer, offset, data, 0, data.Length);
            dataToken.byteArrayForMessage = MergeBytes(dataToken.byteArrayForMessage, data);
            offset += data.Length + 1;
            return true;
        }
    }
}
