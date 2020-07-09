using System;
using System.Net;
using Framework.Net.Sockets;

namespace Framework.Net.Remoting
{
    public abstract class EndpointAddress
    {
        public const int DefaultBufferSize = 2048;
        public const bool DefaultNoDelay = false;

        public EndpointAddress(EndPoint endPoint, int bufferSize, bool noDelay, KeepAlive keepAlive)
        {
            if (endPoint == null)
                throw new ArgumentNullException(nameof(endPoint));
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            EndPoint = endPoint;
            BufferSize = bufferSize;
            NoDelay = noDelay;
            KeepAlive = keepAlive;
        }
        public EndpointAddress(string ip, int port, int bufferSize, bool noDelay, KeepAlive keepAlive)
            : this(new IPEndPoint(IPAddress.Parse(ip), port), bufferSize, noDelay, keepAlive)
        { }
        public EndpointAddress(string ip, int port, int bufferSize = DefaultBufferSize, bool noDelay = DefaultNoDelay)
            : this(new IPEndPoint(IPAddress.Parse(ip), port), bufferSize, noDelay, KeepAlive.OFF)
        { }
        public EndpointAddress(EndPoint endPoint, int bufferSize = DefaultBufferSize, bool noDelay = DefaultNoDelay)
            : this(endPoint, bufferSize, noDelay, KeepAlive.OFF)
        { }

        public EndPoint EndPoint { get; private set; }
        public int BufferSize { get; private set; }
        public bool NoDelay { get; private set; }
        public KeepAlive KeepAlive { get; }
    }
}