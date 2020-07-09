using System;

namespace Framework.Net.WebSockets
{
    internal class MessageHeadFrame
    {
        private readonly byte[] data;

        public static MessageHeadFrame Parse(byte[] data, int offset = 0)
        {
            if (data.Length - offset > 1)
            {
                return new MessageHeadFrame(data, offset);
            }
            return null;
        }

        private MessageHeadFrame(byte[] data, int offset)
        {
            this.data = new byte[2];
            Buffer.BlockCopy(data, offset, this.data, 0, this.data.Length);
        }

        /// <summary>
        /// RSV is 0
        /// </summary>
        public bool CheckRSV
        {
            get { return ((data[0] & 0x70) == 0x00); }
        }

        /// <summary>
        /// Is Finsh
        /// </summary>
        public bool FIN
        {
            get { return ((data[0] & 0x80) == 0x80); }
        }

        /// <summary>
        /// custom protocol
        /// </summary>
        public bool RSV1
        {
            get { return ((data[0] & 0x40) == 0x40); }
        }

        /// <summary>
        /// custom protocol
        /// </summary>
        public bool RSV2
        {
            get { return ((data[0] & 0x20) == 0x20); }
        }

        /// <summary>
        /// custom protocol
        /// </summary>
        public bool RSV3
        {
            get { return ((data[0] & 0x10) == 0x10); }
        }

        /// <summary>
        /// op code:
        /// 0: continue
        /// 1: text message
        /// 2: bir message
        /// 3-7: no use
        /// 8: close connect
        /// 9: ping message
        /// A: pong message
        /// B-F: no use
        /// </summary>
        public sbyte OpCode
        {
            get { return (sbyte)(data[0] & 0x0f); }
        }

        /// <summary>
        /// has mask
        /// </summary>
        public bool HasMask
        {
            get { return ((data[1] & 0x80) == 0x80); }
        }

        /// <summary>
        /// message length
        /// </summary>
        public sbyte PayloadLenght
        {
            get { return (sbyte)(data[1] & 0x7f); }
        }
    }
}
