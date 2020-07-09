using System.Net;

namespace Framework.Net.Sockets
{
    /// <summary>
    /// Socket send async result
    /// </summary>
    public class SocketAsyncResult
    {
        public SocketAsyncResult(byte[] data)
        {
            Data = data;
        }

        public byte[] Data { get; }

        public ExSocket Socket { get; set; }

        public EndPoint RemoteEndPoint { get; internal set; }
    }
}