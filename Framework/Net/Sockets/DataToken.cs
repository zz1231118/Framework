using System;

namespace Framework.Net.Sockets
{
    public class DataToken
    {
        public const int PrefixLength = sizeof(int);
        public const int MaxMessageLength = 10 * 1024 * 1024;

        internal byte[]? ByteArrayForMessage;
        internal byte[] ByteArrayForPrefix = new byte[PrefixLength];
        internal int MessageBytesDone;
        internal int PrefixBytesDone;
        internal int MessageLength;

        public ExSocket? Socket { get; internal set; }

        public int RemainByte => MessageLength - MessageBytesDone;

        public bool IsPrefixReady => PrefixBytesDone == PrefixLength;

        public bool IsMessageReady => MessageBytesDone == MessageLength;

        public virtual void Reset()
        {
            ByteArrayForMessage = null;
            Array.Clear(ByteArrayForPrefix, 0, ByteArrayForPrefix.Length);
            PrefixBytesDone = 0;
            MessageBytesDone = 0;
            MessageLength = 0;
        }
    }
}