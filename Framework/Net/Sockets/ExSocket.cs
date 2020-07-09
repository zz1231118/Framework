using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Framework.Diagnostics;
using Framework.Log;

namespace Framework.Net.Sockets
{
    /// <inheritdoc />
    public class ExSocket
    {
        private const int NoneSentinel = 0;
        private const int ActiveSentinel = 1;

        private static readonly ILogger logger = Logger.GetLogger<ExSocket>();
        private readonly Socket workSocket;
        private readonly Queue<byte[]> sendQueue = new Queue<byte[]>();
        private readonly EndPoint localEndPoint;
        private readonly EndPoint remoteEndPoint;
        private int isInSending;
        private int isInClosing;

        internal PacketStreamer PacketStreamer = new PacketStreamer();

        internal ExSocket(Socket workSocket)
        {
            this.workSocket = workSocket;

            try
            {
                localEndPoint = this.workSocket.LocalEndPoint;
                remoteEndPoint = this.workSocket.RemoteEndPoint;
            }
            catch (Exception ex)
            {
                logger.Error("ExSocket Init error:{0}", ex);
            }
        }

        /// <inheritdoc />
        public Socket WorkSocket => workSocket;

        /// <inheritdoc />
        public EndPoint LocalEndPoint => localEndPoint;

        /// <inheritdoc />
        public EndPoint RemoteEndPoint => remoteEndPoint;

        /// <inheritdoc />
        public bool Connected => workSocket.Connected;

        /// <inheritdoc />
        public bool IsClosed => isInClosing == ActiveSentinel;

        /// <inheritdoc />
        public Guid Guid { get; } = Guid.NewGuid();

        /// <inheritdoc />
        public DateTime LastAccessTime { get; internal set; }

        /// <inheritdoc />
        public int WaitSendPacketCount => sendQueue.Count;

        /// <inheritdoc />
        public object UserToken { get; set; }

        internal bool DirectSendOrEnqueue(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            lock (this)
            {
                sendQueue.Enqueue(data);
                return Interlocked.CompareExchange(ref isInSending, ActiveSentinel, NoneSentinel) == NoneSentinel;
            }
        }

        internal bool TryDequeueOrReset(out byte[] data)
        {
            lock (this)
            {
                if (sendQueue.Count > 0)
                {
                    data = sendQueue.Dequeue();
                    return true;
                }

                data = null;
                Interlocked.Exchange(ref isInSending, NoneSentinel);
                return false;
            }
        }

        /// <summary>
        /// Close socket
        /// </summary>
        public virtual bool Close()
        {
            if (Interlocked.CompareExchange(ref isInClosing, ActiveSentinel, NoneSentinel) == NoneSentinel)
            {
                try
                {
                    WorkSocket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException)
                { }
                catch (Exception ex)
                {
                    var stackFrameString = StackTrace.GetStackFrameString();
                    logger.Error("ExSocket EndPoint:{0} Close error:{1} stack:{2}", RemoteEndPoint, ex, stackFrameString);
                }
                finally
                {
                    WorkSocket.Close();
                }

                return true;
            }

            return false;
        }
    }
}