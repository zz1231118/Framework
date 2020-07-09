using System;
using System.Net.Sockets;
using System.Text;
using Framework.Net.Sockets;

namespace Framework.Net.WebSockets
{
    public class WebSocket : ExSocket
    {
        private const string PingMessage = "ping";
        private const string PongMessage = "pong";
        private readonly WebSocketListener socketListener;
        private readonly DateTime connectionTime;
        private DateTime startCloseTime;

        internal WebSocket(WebSocketListener socketListener, Socket socket)
            : base(socket)
        {
            this.socketListener = socketListener;
            connectionTime = DateTime.Now;
        }

        internal DateTime ConnectionTime => connectionTime;

        internal DateTime StartCloseTime => startCloseTime;

        internal HandshakeData Handshake { get; set; }

        internal void PostSend(byte[] data)
        {
            socketListener.SendAsync(this, data);
        }

        internal void PostSend(string message)
        {
            var encoding = socketListener.Setting.Encoding;
            var data = encoding.GetBytes(message);
            socketListener.SendAsync(this, data);
        }

        public void SendPing()
        {
            byte[] data = Encoding.UTF8.GetBytes(PingMessage);
            socketListener.PostSend(this, Opcode.Ping, data, 0, data.Length);
        }

        public void SendPong()
        {
            byte[] data = Encoding.UTF8.GetBytes(PongMessage);
            socketListener.PostSend(this, Opcode.Pong, data, 0, data.Length);
        }

        public void SendMessage(byte[] data, int offset, int count)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            socketListener.PostSend(this, Opcode.Binary, data, offset, count);
        }

        public void SendMessage(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var encoding = socketListener.Setting.Encoding;
            var data = encoding.GetBytes(message);
            socketListener.PostSend(this, Opcode.Text, data, 0, data.Length);
        }

        public override bool Close()
        {
            if (base.Close())
            {
                startCloseTime = DateTime.Now;
                return true;
            }
            return false;
        }
    }
}
