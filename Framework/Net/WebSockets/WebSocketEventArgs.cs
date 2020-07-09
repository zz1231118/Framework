using System;

namespace Framework.Net.WebSockets
{
    public class WebSocketEventArgs : EventArgs
    {
        internal WebSocketEventArgs(WebSocket socket)
        {
            Socket = socket;
        }

        internal WebSocketEventArgs(WebSocket socket, DataMessage message)
        {
            Socket = socket;
            Message = message;
        }

        public WebSocket Socket { get; }

        public DataMessage Message { get; }
    }
}