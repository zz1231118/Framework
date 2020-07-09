using System;
using System.Net.Sockets;

namespace Framework.Net.Sockets
{
    public class SocketEventArgs : EventArgs
    {
        internal SocketEventArgs(SocketError socketError)
        {
            SocketError = socketError;
        }
        internal SocketEventArgs(ExSocket socket, SocketError socketError)
        {
            Socket = socket;
            SocketError = socketError;
        }
        internal SocketEventArgs(ExSocket socket, SocketError socketError, byte[] data)
            : this(socket, socketError)
        {
            Data = data;
        }

        public ExSocket Socket { get; }
        public SocketError SocketError { get; }
        public byte[] Data { get; }
    }
}