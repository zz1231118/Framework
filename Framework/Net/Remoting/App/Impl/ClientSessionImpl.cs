using System;
using System.Net;
using Framework.Net.Sockets;

namespace Framework.Net.Remoting.App.Impl
{
    class ClientSessionImpl : IClientSession, ISession
    {
        private readonly SocketListener _socketListener;
        private readonly IHostContext _appContext;
        private readonly ExSocket _exSocket;
        private readonly object _command;

        public ClientSessionImpl(SocketListener socketListener, IHostContext appContext, ExSocket exSocket, object command)
        {
            _socketListener = socketListener;
            _appContext = appContext;
            _exSocket = exSocket;
            _command = command;
        }

        public bool IsConnected => _exSocket.Connected;
        public IHostContext Context => _appContext;
        public Guid HashCode => _exSocket.Guid;
        public EndPoint LocalEndPoint => _exSocket.LocalEndPoint;
        public EndPoint RemoteEndPoint => _exSocket.RemoteEndPoint;
        public DateTime LastActivityTime { get; private set; }
        public object Command => _command;

        public void Refresh()
        {
            LastActivityTime = DateTime.Now;
        }
        public void Send(byte[] data, int offset, int count)
        {
            _socketListener.Send(_exSocket, data, offset, count);
        }
        public void Close()
        {
            _exSocket.Close();
        }
    }
}