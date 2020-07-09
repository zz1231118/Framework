using System.Text;

namespace Framework.Net.WebSockets
{
    public class DataMessage
    {
        internal short Opcode { get; set; }

        public byte[] Data { get; internal set; }

        public string Message => Encoding.UTF8.GetString(Data);

        public DataMessageType Type
        {
            get
            {
                switch (Opcode)
                {
                    case WebSockets.Opcode.Binary:
                        return DataMessageType.Binary;
                    case WebSockets.Opcode.Text:
                        return DataMessageType.Text;
                    case WebSockets.Opcode.Close:
                        return DataMessageType.Close;
                    default:
                        return (DataMessageType)(-1);
                }
            }
        }
    }
}
