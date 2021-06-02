using System.Collections.Generic;
using Framework.Net.Sockets;

namespace Framework.Net.WebSockets
{
    internal class WSDataToken : DataToken
    {
        public List<byte>? ByteArrayForHandshake;
        public byte[]? ByteArrayForPrefix2;
        public byte[]? ByteArrayMask;

        public int MaskBytesDone;
        public int PrefixBytesDone2;

        public readonly List<DataSegmentFrame> DataFrames = new List<DataSegmentFrame>();
        public MessageHeadFrame? HeadFrame;

        public override void Reset()
        {
            base.Reset();
            ByteArrayForHandshake = null;
            ByteArrayForPrefix2 = null;
            ByteArrayMask = null;
            MaskBytesDone = 0;
            PrefixBytesDone2 = 0;
            HeadFrame = null;
        }
    }
}
