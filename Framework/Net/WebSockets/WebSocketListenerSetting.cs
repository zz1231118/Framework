using System;
using System.Net;
using System.Text;

namespace Framework.Net.WebSockets
{
    public class WebSocketListenerSetting
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public int Version { get; set; } = 13;

        public IPEndPoint EndPoint { get; set; } = new IPEndPoint(IPAddress.Any, 8080);

        public int Backlog { get; set; } = int.MaxValue;

        public int MaxAcceptOps { get; set; }

        public int MaxConnections { get; set; }

        public int BufferSize { get; set; } = 20480;

        public TimeSpan HandshakePendingQueueCheckingInterval { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan OpenHandshakeTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public TimeSpan CloseHandshakeTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public bool IsSecurity { get; set; }

        public bool IsMask { get; set; }
    }
}
