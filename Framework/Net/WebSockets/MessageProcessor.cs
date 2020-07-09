using System.Collections.Generic;

namespace Framework.Net.WebSockets
{
    internal abstract class MessageProcessor
    {
        public bool IsMask { get; set; }

        public CloseStatusCode CloseStatusCode { get; set; }

        protected MessageProcessor()
        { }

        public abstract bool TryReadMeaage(WSDataToken dataToken, byte[] buffer, out List<DataMessage> messageList);

        public abstract byte[] BuildMessagePack(WebSocket exSocket, sbyte opCode, byte[] data, int offset, int count);

        public abstract byte[] CloseMessage(WebSocket exSocket, sbyte opCode, string reason);

        public int GetCloseStatus(byte[] data)
        {
            if (data == null || data.Length <= 1)
            {
                return Opcode.Empty;
            }
            var code = data[0] * 256 + data[1];

            if (!IsValidCloseCode(code))
            {
                return Opcode.Empty;
            }
            return code;
        }

        protected abstract bool IsValidCloseCode(int code);
    }
}
