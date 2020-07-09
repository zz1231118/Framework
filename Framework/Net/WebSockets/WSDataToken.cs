using System.Collections.Generic;
using Framework.Net.Sockets;

namespace Framework.Net.WebSockets
{
    internal class WSDataToken : DataToken
    {
        public List<byte> byteArrayForHandshake;
        public byte[] byteArrayForPrefix2;
        public byte[] byteArrayMask;

        public int maskBytesDone;
        public int prefixBytesDone2;

        public MessageHeadFrame HeadFrame;
        public List<DataSegmentFrame> DataFrames = new List<DataSegmentFrame>();

        public override void Reset()
        {
            base.Reset();
            byteArrayForHandshake = null;
            byteArrayForPrefix2 = null;
            byteArrayMask = null;
            maskBytesDone = 0;
            prefixBytesDone2 = 0;
            HeadFrame = null;
        }
    }
}
