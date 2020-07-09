using System;
using System.Net;
using Framework.Net.Sockets;

namespace Framework.Net.Remoting
{
    public class ClientEndpoint : EndpointAddress
    {
        private TimeSpan _timeout = TimeSpan.FromHours(2);

        public ClientEndpoint(EndPoint endPoint, int bufferSize, bool noDelay, KeepAlive keepAlive)
            : base(endPoint, bufferSize, noDelay, keepAlive)
        { }
        public ClientEndpoint(string ip, int port, int bufferSize, bool noDelay, KeepAlive keepAlive)
            : base(ip, port, bufferSize, noDelay, keepAlive)
        { }
        public ClientEndpoint(string ip, int port, int bufferSize = DefaultBufferSize, bool noDelay = DefaultNoDelay)
            : base(ip, port, bufferSize, noDelay)
        { }
        public ClientEndpoint(EndPoint endPoint, int bufferSize = DefaultBufferSize, bool noDelay = DefaultNoDelay)
            : base(endPoint, bufferSize, noDelay)
        { }

        public TimeSpan Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }
    }
}