namespace Framework.Net.WebSockets
{
    static class HandshakeHeadKeys
    {
        public const string Method = "GET";

        public const string HttpVersion = "HTTP/1.1";

        public const string Host = "Host";

        public const string Connection = "Connection";

        public const string Cookie = "Cookie";

        public const string Upgrade = "Upgrade";

        public const string Origin = "Origin";

        public const string SecSignKey = "Sec-WebSocket-SignKey";

        public const string SecAccept = "Sec-WebSocket-Accept";

        public const string SecKey = "Sec-WebSocket-Key";

        public const string SecKey1 = "Sec-WebSocket-Key1";

        public const string SecKey2 = "Sec-WebSocket-Key2";

        public const string SecKey3 = "Sec-WebSocket-Key3";

        public const string SecVersion = "Sec-WebSocket-Version";

        public const string SecProtocol = "Sec-WebSocket-Protocol";

        public const string SecExtensions = "Sec-WebSocket-Extensions";

        public const string Protocol = "WebSocket-Protocol";

        public const string RespHead_00 = "HTTP/1.1 101 WebSocket Protocol Handshake";

        public const string RespHead_10 = "HTTP/1.1 101 Switching Protocols";

        public const string RespUpgrade = Upgrade + ": websocket";

        public const string RespUpgrade00 = Upgrade + ": WebSocket";

        public const string RespConnection = Connection + ": Upgrade";

        public const string RespOriginLine = "Sec-WebSocket-Origin: {0}";

        public const string RespUrl = "{0}://{1}{2}";

        public const string SecLocation = "Sec-WebSocket-Location: " + RespUrl;

        public const string RespProtocol = SecProtocol + ": {0}";

        public const string RespAccept = SecAccept + ": {0}";
    }
}
