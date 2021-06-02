using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.Net.WebSockets
{
    class Hybi10MessageProcessor : Hybi00MessageProcessor
    {
        private const int Version = 8;
        private const int PreByteLength = 2;
        private const int MaskLength = 4;

        public Hybi10MessageProcessor()
        {
            CloseStatusCode = new Hybi10CloseStatusCode();
        }

        protected bool CheckVersion(WebSocket exSocket)
        {
            return exSocket != null &&
                exSocket.Handshake != null &&
                exSocket.Handshake.WebSocketVersion < Version;
        }

        public override bool TryReadMeaage(WSDataToken dataToken, byte[] buffer, out List<DataMessage> messageList)
        {
            if (CheckVersion((WebSocket)dataToken.Socket))
            {
                return base.TryReadMeaage(dataToken, buffer, out messageList);
            }
            messageList = new List<DataMessage>();
            int offset = 0;
            do
            {
                //receive buffer is complated
                if (!CheckPrefixHeadComplated(dataToken, buffer, ref offset) ||
                    !dataToken.HeadFrame.CheckRSV ||
                    !CheckPayloadHeadComplated(dataToken, buffer, ref offset) ||
                    !CheckPayloadDataComplated(dataToken, buffer, ref offset))
                {
                    return false;
                }
                byte[] data = dataToken.HeadFrame.HasMask
                        ? DecodeMask(dataToken.ByteArrayForMessage, dataToken.ByteArrayMask, 0, dataToken.MessageLength)
                        : dataToken.ByteArrayForMessage;

                if (!dataToken.HeadFrame.FIN)
                {
                    dataToken.DataFrames.Add(new DataSegmentFrame()
                    {
                        Head = dataToken.HeadFrame,
                        Data = new ArraySegment<byte>(data)
                    });
                }
                else
                {
                    //frame complated
                    sbyte opCode;
                    if (dataToken.DataFrames.Count > 0)
                    {
                        dataToken.DataFrames.Add(new DataSegmentFrame()
                        {
                            Head = dataToken.HeadFrame,
                            Data = new ArraySegment<byte>(data)
                        });
                        opCode = dataToken.DataFrames[0].Head.OpCode;
                        data = CombineDataFrames(dataToken.DataFrames);
                    }
                    else
                    {
                        opCode = dataToken.HeadFrame.OpCode;
                    }
                    messageList.Add(new DataMessage() { Data = data, Opcode = opCode });
                    dataToken.DataFrames.Clear();
                }
                dataToken.Reset();
            } while (offset < buffer.Length);

            return true;
        }

        public override byte[] BuildMessagePack(WebSocket exSocket, sbyte opCode, byte[] data, int offset, int count)
        {
            if (CheckVersion(exSocket))
            {
                return base.BuildMessagePack(exSocket, opCode, data, offset, count);
            }
            bool isMask = IsMask;
            int maskNum = isMask ? MaskLength : 0;
            byte[] buffer;
            if (count < 126)
            {
                buffer = new byte[count + maskNum + 2];
                //buffer[0] = 0x81;
                buffer[1] = (byte)count;
                //Buffer.BlockCopy(data, offset, buffer, 2, count);
            }
            else if (count < 0xFFFF)
            {
                //uint16 bit
                buffer = new byte[count + maskNum + 4];
                //buffer[0] = 0x81;
                buffer[1] = 126;
                buffer[2] = (byte)(count / 256);
                buffer[3] = (byte)(count % 256);
                //Buffer.BlockCopy(data, offset, buffer, 4, count);
            }
            else
            {
                //uint64 bit
                buffer = new byte[count + maskNum + 10];
                //buffer[0] = 0x81;
                buffer[1] = 127;
                int num2 = count;
                int num3 = 256;
                for (int i = 9; i > 1; i--)
                {
                    buffer[i] = (byte)(num2 % num3);
                    num2 /= num3;
                    if (num2 == 0)
                    {
                        break;
                    }
                }
            }
            if (isMask)
            {
                //mask after of payloadLength 
                byte[] mask = GenerateMask();
                int maskPos = buffer.Length - maskNum - count;
                Buffer.BlockCopy(mask, 0, buffer, maskPos, mask.Length);
                EncodeMask(data, offset, count, mask, buffer, buffer.Length - count);
            }
            else
            {
                Buffer.BlockCopy(data, offset, buffer, buffer.Length - count, count);
            }
            buffer[0] = (byte)((byte)opCode | 0x80);
            if (isMask)
            {
                buffer[1] = (byte)(buffer[1] | 0x80);
            }
            return buffer;
        }

        public override byte[] CloseMessage(WebSocket exSocket, sbyte opCode, string reason)
        {
            byte[] data = Encoding.UTF8.GetBytes(reason);
            return BuildMessagePack(exSocket, opCode, data, 0, data.Length);
        }

        protected override bool IsValidCloseCode(int code)
        {
            var closeCode = CloseStatusCode;

            if (code >= 0 && code <= 999)
                return false;

            if (code >= 1000 && code <= 1999)
            {
                if (code == closeCode.NormalClosure
                    || code == closeCode.GoingAway
                    || code == closeCode.ProtocolError
                    || code == closeCode.UnexpectedCondition
                    //|| code == closeCode.Reserved
                    //|| code == closeCode.NoStatusRcvd
                    || code == closeCode.AbnormalClosure
                    || code == closeCode.InvalidUTF8
                    || code == closeCode.PolicyViolation
                    || code == closeCode.MessageTooBig
                    || code == closeCode.MandatoryExt)
                {
                    return true;
                }
                return false;
            }
            //2000-2999 use by extensions
            //3000-3999 libraries and frameworks
            //4000-4999 application code
            if (code >= 2000 && code <= 4999)
                return true;

            return false;
        }

        private byte[] CombineDataFrames(List<DataSegmentFrame> dataFrames)
        {
            int len = dataFrames.Sum(t => t.Head.PayloadLenght);
            byte[] buffer = new byte[len];
            int offset = 0;
            foreach (var frame in dataFrames)
            {
                Buffer.BlockCopy(frame.Data.Array, frame.Data.Offset, buffer, offset, frame.Head.PayloadLenght);
                offset += frame.Head.PayloadLenght;
            }
            return buffer;
        }

        internal byte[] GenerateMask()
        {
            var random = new Random();
            var mask = new byte[MaskLength];
            for (int i = 0; i < MaskLength; i++)
            {
                mask[i] = (byte)random.Next(0, 255);
            }
            return mask;
        }

        internal void EncodeMask(byte[] rawData, int rowOffset, int rowCount, byte[] mask, byte[] buffer, int offset)
        {
            for (int i = 0; i < rowCount; i++)
            {
                int num = rowOffset + i;
                buffer[offset++] = (byte)(rawData[num] ^ mask[i % 4]);
            }
        }


        internal byte[] DecodeMask(byte[] buffer, byte[] maskKey, int offset, int count)
        {
            for (var i = offset; i < count; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ maskKey[i % maskKey.Length]);
            }
            return buffer;
        }

        private bool CheckPayloadDataComplated(WSDataToken dataToken, byte[] buffer, ref int offset)
        {
            int copyByteCount = dataToken.RemainByte;
            if (buffer.Length - offset >= copyByteCount)
            {
                Buffer.BlockCopy(buffer, offset, dataToken.ByteArrayForMessage, dataToken.MessageBytesDone, copyByteCount);
                dataToken.MessageBytesDone += copyByteCount;
                offset += copyByteCount;
            }
            else
            {
                Buffer.BlockCopy(buffer, offset, dataToken.ByteArrayForMessage, dataToken.MessageBytesDone, buffer.Length - offset);
                dataToken.MessageBytesDone += buffer.Length - offset;
                offset += buffer.Length - offset;
            }
            return dataToken.IsMessageReady;
        }

        private bool CheckPrefixHeadComplated(WSDataToken dataToken, byte[] buffer, ref int offset)
        {
            if (dataToken.ByteArrayForPrefix == null || dataToken.ByteArrayForPrefix.Length != PreByteLength)
            {
                dataToken.ByteArrayForPrefix = new byte[PreByteLength];
            }
            if (PreByteLength - dataToken.PrefixBytesDone > buffer.Length - offset)
            {
                Buffer.BlockCopy(buffer, offset, dataToken.ByteArrayForPrefix, dataToken.PrefixBytesDone, buffer.Length - offset);
                dataToken.PrefixBytesDone += buffer.Length - offset;
                return false;
            }

            int count = dataToken.ByteArrayForPrefix.Length - dataToken.PrefixBytesDone;
            if (count > 0)
            {
                Buffer.BlockCopy(buffer, offset, dataToken.ByteArrayForPrefix, dataToken.PrefixBytesDone, count);
                dataToken.PrefixBytesDone += count;
                offset += count;
            }
            if (dataToken.HeadFrame == null)
            {
                //build message head
                dataToken.HeadFrame = MessageHeadFrame.Parse(dataToken.ByteArrayForPrefix);
            }
            return true;
        }

        private bool CheckPayloadHeadComplated(WSDataToken dataToken, byte[] buffer, ref int offset)
        {
            try
            {
                if (dataToken.ByteArrayForMessage == null)
                {
                    int size = 0;
                    int payloadLenght = dataToken.HeadFrame.PayloadLenght;
                    switch (payloadLenght)
                    {
                        case 126:
                            size = 2; //uint16 2bit
                            if (!CheckPrefix2Complated(dataToken, buffer, ref offset, size)) return false;
                            UInt16 len = (UInt16)(dataToken.ByteArrayForPrefix2[0] << 8 | dataToken.ByteArrayForPrefix2[1]);
                            dataToken.ByteArrayForMessage = new byte[len];
                            break;
                        case 127:
                            size = 8; //uint64 8bit
                            if (!CheckPrefix2Complated(dataToken, buffer, ref offset, size)) return false;
                            UInt64 len64 = BitConverter.ToUInt64(dataToken.ByteArrayForPrefix2.Reverse().ToArray(), 0);
                            dataToken.ByteArrayForMessage = new byte[len64];
                            break;
                        default:
                            dataToken.ByteArrayForMessage = new byte[payloadLenght];
                            break;
                    }
                    dataToken.MessageLength = dataToken.ByteArrayForMessage.Length;
                }
                if (dataToken.HeadFrame.HasMask)
                {
                    if (dataToken.ByteArrayMask == null || dataToken.ByteArrayMask.Length != MaskLength)
                    {
                        dataToken.ByteArrayMask = new byte[MaskLength];
                    }
                    if (MaskLength - dataToken.MaskBytesDone > buffer.Length - offset)
                    {
                        Buffer.BlockCopy(buffer, offset, dataToken.ByteArrayMask, dataToken.MaskBytesDone, buffer.Length - offset);
                        dataToken.MaskBytesDone += buffer.Length - offset;
                        return false;
                    }
                    int count = dataToken.ByteArrayMask.Length - dataToken.MaskBytesDone;
                    if (count > 0)
                    {
                        Buffer.BlockCopy(buffer, offset, dataToken.ByteArrayMask, dataToken.MaskBytesDone, count);
                        dataToken.MaskBytesDone += count;
                        offset += count;
                    }
                }
                return true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool CheckPrefix2Complated(WSDataToken dataToken, byte[] buffer, ref int offset, int size)
        {
            if (dataToken.ByteArrayForPrefix2 == null || dataToken.ByteArrayForPrefix2.Length != size)
            {
                dataToken.ByteArrayForPrefix2 = new byte[size];
            }
            if (size - dataToken.PrefixBytesDone2 > buffer.Length - offset)
            {
                Buffer.BlockCopy(buffer, offset, dataToken.ByteArrayForPrefix2, dataToken.PrefixBytesDone2, buffer.Length - offset);
                dataToken.PrefixBytesDone2 += buffer.Length - offset;
                return false;
            }
            int count = dataToken.ByteArrayForPrefix2.Length - dataToken.PrefixBytesDone2;
            if (count > 0)
            {
                Buffer.BlockCopy(buffer, offset, dataToken.ByteArrayForPrefix2, dataToken.PrefixBytesDone2, count);
                dataToken.PrefixBytesDone2 += count;
                offset += count;
            }
            return true;
        }
    }
}
