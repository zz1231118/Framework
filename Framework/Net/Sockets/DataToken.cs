using System;

namespace Framework.Net.Sockets
{
    public class DataToken
    {
        public const int PrefixLength = sizeof(int);
        public const int MaxMessageLength = 10 * 1024 * 1024;

        internal byte[] byteArrayForMessage;
        internal byte[] byteArrayForPrefix = new byte[PrefixLength];
        internal int messageBytesDone;
        internal int prefixBytesDone;
        internal int messageLength;

        public ExSocket Socket { get; internal set; }

        public int RemainByte => messageLength - messageBytesDone;

        public bool IsPrefixReady => prefixBytesDone == PrefixLength;

        public bool IsMessageReady => messageBytesDone == messageLength;

        public virtual void Reset()
        {
            byteArrayForMessage = null;
            Array.Clear(byteArrayForPrefix, 0, byteArrayForPrefix.Length);
            prefixBytesDone = 0;
            messageBytesDone = 0;
            messageLength = 0;
        }
    }
}