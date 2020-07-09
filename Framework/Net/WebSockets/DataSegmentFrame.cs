using System;

namespace Framework.Net.WebSockets
{
    internal class DataSegmentFrame
    {
        public MessageHeadFrame Head { get; set; }

        public ArraySegment<byte> Data { get; set; }
    }
}
