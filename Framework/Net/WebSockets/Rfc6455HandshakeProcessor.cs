using System.Text;

namespace Framework.Net.WebSockets
{
    class Rfc6455HandshakeProcessor : Hybi10HandshakeProcessor
    {
        public Rfc6455HandshakeProcessor(WebSocketListener webSocketListener, Encoding encoding)
            : base(webSocketListener, encoding)
        { }

        public Rfc6455HandshakeProcessor(WebSocketListener webSocketListener)
            : this(webSocketListener, Encoding.UTF8)
        { }
    }
}
