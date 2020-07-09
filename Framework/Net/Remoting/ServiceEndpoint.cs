using System.Net;
using Framework.Net.Sockets;

namespace Framework.Net.Remoting
{
    public class ServiceEndpoint : EndpointAddress
    {
        public ServiceEndpoint(EndPoint endPoint, int backlog, int maxConnection, int bufferSize, bool noDelay, KeepAlive keepAlive)
            : base(endPoint, bufferSize, noDelay, keepAlive)
        {
            Backlog = backlog;
            MaxConnection = maxConnection;
        }
        public ServiceEndpoint(string ip, int port, int backlog, int maxConnection, int bufferSize, bool noDelay, KeepAlive keepAlive)
            : this(new IPEndPoint(IPAddress.Parse(ip), port), backlog, maxConnection, bufferSize, noDelay, keepAlive)
        { }
        public ServiceEndpoint(EndPoint endPoint, int backlog, int maxConnection, int bufferSize = DefaultBufferSize, bool noDelay = DefaultNoDelay)
            : this(endPoint, backlog, maxConnection, bufferSize, noDelay, KeepAlive.OFF)
        { }
        public ServiceEndpoint(int port, int backlog, int maxConnection, int bufferSize = DefaultBufferSize, bool noDelay = DefaultNoDelay)
            : this(new IPEndPoint(IPAddress.Any, port), backlog, maxConnection, bufferSize, noDelay, KeepAlive.OFF)
        { }
        public ServiceEndpoint(string ip, int port, int backlog, int maxConnection, int bufferSize = DefaultBufferSize, bool noDelay = DefaultNoDelay)
            : this(new IPEndPoint(IPAddress.Parse(ip), port), backlog, maxConnection, bufferSize, noDelay, KeepAlive.OFF)
        { }

        public int Backlog { get; private set; }
        public int MaxConnection { get; private set; }
    }
}