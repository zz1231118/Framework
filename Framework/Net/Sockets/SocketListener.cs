using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Framework.Log;
using Framework.Statistics;

namespace Framework.Net.Sockets
{
    /// <summary>
    /// Tcp socket Listener
    /// </summary>
    public class SocketListener : IDisposable
    {
        private const int NoneSentinel = 0;
        private const int ActiveSentinel = 1;

        private static readonly ILogger logger = Logger.GetLogger<SocketListener>();
        private readonly EndPoint listenEndPoint;
        private readonly int backlog;
        private readonly int maxConnections;
        private readonly IPacketProcessor packetProcessor;
        private readonly SocketAsyncEventArgs acceptEventArgs;
        private readonly ConcurrentStack<SocketAsyncEventArgs> ioEventArgsPool;
        private readonly Semaphore maxConnectionsEnforcer;
        private readonly SocketListenerStatistics statistics;
        private SocketOperation sendOperation = SocketOperation.Asynchronization;
        private Socket socket;
        private BufferManager bufferManager;
        private int isInDisposing;
        private int isInActivating;
        private int bufferSize;
        private bool noDelay;
        private KeepAlive keepAlive;
        private TimeSpan sendTimeout;
        private TimeSpan receiveTimeout;

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        public SocketListener(EndPoint listenEndPoint, int backlog, int maxConnections, IPacketProcessor packetProcessor)
        {
            if (listenEndPoint == null)
                throw new ArgumentNullException(nameof(listenEndPoint));
            if (backlog <= 0)
                throw new ArgumentOutOfRangeException(nameof(backlog));
            if (maxConnections <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxConnections));
            if (packetProcessor == null)
                throw new ArgumentNullException(nameof(packetProcessor));

            this.listenEndPoint = listenEndPoint;
            this.backlog = backlog;
            this.maxConnections = maxConnections;
            this.packetProcessor = packetProcessor;
            this.bufferSize = SocketConstants.DefaultBufferSize;
            this.keepAlive = KeepAlive.OFF;
            this.maxConnectionsEnforcer = new Semaphore(maxConnections, maxConnections);
            this.statistics = new SocketListenerStatistics();

            this.acceptEventArgs = new SocketAsyncEventArgs();
            this.acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
            this.ioEventArgsPool = new ConcurrentStack<SocketAsyncEventArgs>();
            var numOfSaeaForRecSend = maxConnections * 2;
            for (int i = 0; i < numOfSaeaForRecSend; i++)
            {
                var ioEventArgs = new SocketAsyncEventArgs();
                ioEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

                var dataToken = new DataToken();
                ioEventArgs.UserToken = dataToken;
                ioEventArgsPool.Push(ioEventArgs);
            }
        }

        /// <inheritdoc />
        public SocketListener(int port, int backlog, int maxConnections, IPacketProcessor packetProcessor)
            : this(new IPEndPoint(IPAddress.Any, port), backlog, maxConnections, packetProcessor)
        { }

        /// <inheritdoc />
        public SocketListener(EndPoint listenEndPoint, int backlog, int maxConnections)
            : this(listenEndPoint, backlog, maxConnections, new DefaultPacketProcessor())
        { }

        /// <inheritdoc />
        public SocketListener(int port, int backlog, int maxConnections)
            : this(new IPEndPoint(IPAddress.Any, port), backlog, maxConnections, new DefaultPacketProcessor())
        { }

        /// <summary>
        /// IsDisposed
        /// </summary>
        protected bool IsDisposed => isInDisposing == ActiveSentinel;

        /// <summary>
        /// Activated
        /// </summary>
        public bool IsActived => isInActivating == ActiveSentinel;

        /// <summary>
        /// Listen endpoint
        /// </summary>
        public EndPoint ListenEndPoint => listenEndPoint;

        /// <summary>
        /// Backlog
        /// </summary>
        public int Backlog => backlog;

        /// <summary>
        /// Max connection count
        /// </summary>
        public int MaxConnections => maxConnections;

        /// <summary>
        /// <para>buffer size</para>
        /// <para>default: <see cref="SocketConstants.DefaultBufferSize"/></para>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ObjectDisposedException" />
        public int BufferSize
        {
            get => bufferSize;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                CheckDisposed();
                if (IsActived)
                {
                    throw new InvalidOperationException("actived");
                }

                bufferSize = value;
            }
        }

        /// <summary>
        /// NoDelay
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ObjectDisposedException" />
        public bool NoDelay
        {
            get => noDelay;
            set
            {
                CheckDisposed();
                if (IsActived)
                {
                    throw new InvalidOperationException("actived");
                }

                noDelay = value;
            }
        }

        /// <summary>
        /// KeepAlive
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ObjectDisposedException" />
        public KeepAlive KeepAlive
        {
            get => keepAlive;
            set
            {
                CheckDisposed();
                if (IsActived)
                {
                    throw new InvalidOperationException("actived");
                }

                keepAlive = value;
            }
        }

        /// <summary>
        /// Send data timeout duration
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ObjectDisposedException" />
        public TimeSpan SendTimeout
        {
            get => sendTimeout;
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(nameof(value));

                CheckDisposed();
                if (IsActived)
                {
                    throw new InvalidOperationException("actived");
                }

                sendTimeout = value;
            }
        }

        /// <summary>
        /// Receive data timeout duration
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ObjectDisposedException" />
        public TimeSpan ReceiveTimeout
        {
            get => receiveTimeout;
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(nameof(value));

                CheckDisposed();
                if (IsActived)
                {
                    throw new InvalidOperationException("actived");
                }

                receiveTimeout = value;
            }
        }

        /// <summary>
        /// <para>Send operation</para>
        /// <para>default: <see cref="SocketOperation.Asynchronization"/></para>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ObjectDisposedException" />
        public SocketOperation SendOperation
        {
            get => sendOperation;
            set
            {
                CheckDisposed();
                if (IsActived)
                {
                    throw new InvalidOperationException("actived");
                }

                sendOperation = value;
            }
        }

        /// <summary>
        /// Statistics counter
        /// </summary>
        public SocketListenerStatistics Statistics => statistics;

        /// <summary>
        /// Available IOEventArgs pool count
        /// </summary>
        public int AvailableIOEventArgsPool => ioEventArgsPool.Count;

        /// <summary>
        /// Connected event
        /// </summary>
        public event EventHandler<SocketEventArgs> Connected;

        /// <summary>
        /// DataReceived event
        /// </summary>
        public event EventHandler<SocketEventArgs> DataReceived;

        /// <summary>
        /// DataReceived event
        /// </summary>
        public event EventHandler<SocketEventArgs> Disconnected;

        void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Accept:
                        ProcessAccept(e);
                        break;
                    default:
                        throw new ArgumentException("The last operation completed on the socket was not a accept");
                }
            }
            catch (Exception ex)
            {
                logger.Error("SocketListener Accept_Completed error:" + ex);
                //throw ex;
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                var dataToken = (DataToken)e.UserToken;
                dataToken.Socket.LastAccessTime = DateTime.Now;
                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Send:
                        ProcessSend(e);
                        break;
                    case SocketAsyncOperation.Receive:
                        ProcessReceive(e);
                        break;
                    default:
                        throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                }
            }
            //catch (ObjectDisposedException)
            //{
            //    //TraceLog.WriteError("SocketListener IO_Completed error:" + ex);
            //    ReleaseIOEventArgs(e);
            //    maxConnectionsEnforcer.Release();
            //}
            catch (Exception ex)
            {
                logger.Error("SocketListener IO_Completed error:" + ex);
                //throw ex;
            }
        }

        private void PostAccept()
        {
            if (!IsActived)
            {
                //not start
                return;
            }

            maxConnectionsEnforcer.WaitOne();
            bool willRaiseEvent;
            try
            {
                willRaiseEvent = socket.AcceptAsync(acceptEventArgs);
            }
            catch (ObjectDisposedException)
            {
                acceptEventArgs.Dispose();
                return;
            }
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArgs);
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            try
            {
                statistics.inboundConnectionCounter.Increment();
                if (acceptEventArgs.SocketError != SocketError.Success)
                {
                    ReleaseAcceptEventArgs(acceptEventArgs);
                    maxConnectionsEnforcer.Release();
                    statistics.rejectedConnectionCounter.Increment();
                    return;
                }
                if (!ioEventArgsPool.TryPop(out SocketAsyncEventArgs ioEventArgs))
                {
                    ReleaseAcceptEventArgs(acceptEventArgs);
                    maxConnectionsEnforcer.Release();
                    statistics.rejectedConnectionCounter.Increment();
                    logger.Error("apply for IoEventArgs failed...");
                    return;
                }

                ioEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;
                acceptEventArgs.AcceptSocket = null;

                var ioSocket = ioEventArgs.AcceptSocket;
                ioSocket.NoDelay = noDelay;
                if (keepAlive != KeepAlive.OFF) keepAlive.IOControl(ioSocket);
                if (sendTimeout > TimeSpan.Zero) ioSocket.SendTimeout = (int)sendTimeout.TotalMilliseconds;
                if (receiveTimeout > TimeSpan.Zero) ioSocket.ReceiveTimeout = (int)receiveTimeout.TotalMilliseconds;

                var newSocket = new ExSocket(ioSocket);
                newSocket.LastAccessTime = DateTime.Now;

                var dataToken = (DataToken)ioEventArgs.UserToken;
                dataToken.Socket = newSocket;
                dataToken.Reset();
                ioEventArgs.SetBuffer(ioEventArgs.Offset, bufferSize);

                statistics.currentConnectionCounter.Increment();
                NotifyConnectedEvent(new SocketEventArgs(newSocket, SocketError.Success));
                if (newSocket.IsClosed)
                {
                    DoClosed(ioEventArgs, SocketError.OperationAborted);
                    return;
                }

                PostReceive(ioEventArgs);
            }
            finally
            {
                PostAccept();
            }
        }

        private void PostReceive(SocketAsyncEventArgs ioEventArgs)
        {
            bool willRaiseEvent;
            try
            {
                willRaiseEvent = ioEventArgs.AcceptSocket.ReceiveAsync(ioEventArgs);
            }
            catch (ObjectDisposedException)
            {
                DoClosed(ioEventArgs, SocketError.OperationAborted);
                return;
            }
            if (!willRaiseEvent)
            {
                ProcessReceive(ioEventArgs);
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs ioEventArgs)
        {
            if (ioEventArgs.SocketError == SocketError.OperationAborted)
            {
                DoClosed(ioEventArgs, ioEventArgs.SocketError);
                return;
            }
            if (ioEventArgs.SocketError != SocketError.Success)
            {
                DoClosing(ioEventArgs, ioEventArgs.SocketError);
                return;
            }
            if (ioEventArgs.BytesTransferred == 0)
            {
                DoClosing(ioEventArgs, SocketError.ConnectionReset);
                return;
            }

            statistics.recvConcurrencyCounter.Increment();
            var dataToken = (DataToken)ioEventArgs.UserToken;
            var exSocket = dataToken.Socket;
            var packetStreamer = exSocket.PacketStreamer;
            if (!packetProcessor.HandlePacket(ioEventArgs, packetStreamer))
            {
                DoClosing(ioEventArgs, SocketError.SocketError);
                return;
            }
            while (packetStreamer.TryDequeue(out byte[] bytes))
            {
                NotifyDataReceivedEvent(new SocketEventArgs(dataToken.Socket, SocketError.Success, bytes));
            }

            statistics.recvConcurrencyCounter.Decrement();
            if (exSocket.IsClosed)
            {
                DoClosed(ioEventArgs, SocketError.OperationAborted);
                return;
            }

            PostReceive(ioEventArgs);
        }

        private void TryDequeueAndPostSend(ExSocket socket, SocketAsyncEventArgs ioEventArgs)
        {
            if (socket.TryDequeueOrReset(out byte[] data))
            {
                var dataToken = (DataToken)ioEventArgs.UserToken;
                dataToken.Socket = socket;
                dataToken.byteArrayForMessage = data;
                dataToken.messageLength = data.Length;
                PostSend(ioEventArgs);
            }
            else
            {
                ReleaseIOEventArgs(ioEventArgs);
                statistics.sendConcurrencyCounter.Decrement();
            }
        }

        private void PostSend(SocketAsyncEventArgs ioEventArgs)
        {
            var dataToken = (DataToken)ioEventArgs.UserToken;
            var differBytes = dataToken.messageLength - dataToken.messageBytesDone;
            var copyedBytes = bufferSize <= differBytes ? bufferSize : differBytes;
            ioEventArgs.SetBuffer(ioEventArgs.Offset, copyedBytes);
            Buffer.BlockCopy(dataToken.byteArrayForMessage, dataToken.messageBytesDone, ioEventArgs.Buffer, ioEventArgs.Offset, copyedBytes);

            bool willRaiseEvent;
            try
            {
                willRaiseEvent = ioEventArgs.AcceptSocket.SendAsync(ioEventArgs);
            }
            catch (ObjectDisposedException)
            {
                ReleaseIOEventArgs(ioEventArgs);
                statistics.sendConcurrencyCounter.Decrement();
                return;
            }
            if (!willRaiseEvent)
            {
                ProcessSend(ioEventArgs);
            }
        }

        private void ProcessSend(SocketAsyncEventArgs ioEventArgs)
        {
            if (ioEventArgs.SocketError != SocketError.Success)
            {
                ReleaseIOEventArgs(ioEventArgs);
                statistics.sendConcurrencyCounter.Decrement();
                return;
            }
            var dataToken = (DataToken)ioEventArgs.UserToken;
            dataToken.messageBytesDone += ioEventArgs.BytesTransferred;
            if (dataToken.messageBytesDone < dataToken.messageLength)
            {
                PostSend(ioEventArgs);
            }
            else
            {
                dataToken.Reset();
                TryDequeueAndPostSend(dataToken.Socket, ioEventArgs);
            }
        }

        private void ResetIOSocketEventArgs(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            var dataToken = (DataToken)socketAsyncEventArgs.UserToken;
            var socket = dataToken.Socket;
            if (socket != null) socket.Close();
        }

        private void ReleaseAcceptEventArgs(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs.AcceptSocket != null)
            {
                try
                {
                    acceptEventArgs.AcceptSocket.Close();
                }
                catch (Exception ex)
                {
                    logger.Error("SocketListener ResetIOEventArgs error:" + ex);
                }

                acceptEventArgs.AcceptSocket = null;
            }
            if (IsDisposed) acceptEventArgs.Dispose();
        }

        private void ReleaseIOEventArgs(SocketAsyncEventArgs ioEventArgs)
        {
            var dataToken = (DataToken)ioEventArgs.UserToken;
            dataToken.Socket = null;
            dataToken.Reset();
            ioEventArgs.AcceptSocket = null;
            if (IsDisposed) ioEventArgs.Dispose();
            else ioEventArgsPool.Push(ioEventArgs);
        }

        private void DoClosing(SocketAsyncEventArgs ioEventArgs, SocketError socketError = SocketError.Success)
        {
            ResetIOSocketEventArgs(ioEventArgs);
            DoClosed(ioEventArgs, socketError);
        }

        private void DoClosed(SocketAsyncEventArgs ioEventArgs, SocketError socketError = SocketError.Success)
        {
            var dataToken = (DataToken)ioEventArgs.UserToken;
            var exSocket = dataToken.Socket;
            NotifyDisconnectedEvent(new SocketEventArgs(exSocket, socketError));
            ReleaseIOEventArgs(ioEventArgs);
            maxConnectionsEnforcer.Release();
            statistics.closedConnectionCounter.Increment();
            statistics.currentConnectionCounter.Decrement();
        }

        private void NotifyConnectedEvent(SocketEventArgs e)
        {
            var connect = Connected;
            if (connect != null)
            {
                try
                {
                    connect(this, e);
                }
                catch (Exception ex)
                {
                    logger.Error("SocketListener Connected event error:" + ex.ToString());
                }
            }
        }

        private void NotifyDataReceivedEvent(SocketEventArgs e)
        {
            var dataReceive = DataReceived;
            if (dataReceive != null)
            {
                try
                {
                    dataReceive(this, e);
                }
                catch (Exception ex)
                {
                    logger.Error("SocketListener DataReceived event error:" + ex.ToString());
                }
            }
        }

        private void NotifyDisconnectedEvent(SocketEventArgs e)
        {
            var disconnect = Disconnected;
            if (disconnect != null)
            {
                try
                {
                    disconnect(this, e);
                }
                catch (Exception ex)
                {
                    logger.Error("SocketListener Disconnected event error:" + ex.ToString());
                }
            }
        }

        /// <inheritdoc />
        protected void CheckDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref isInDisposing, ActiveSentinel, NoneSentinel) == NoneSentinel)
            {
                Connected = null;
                DataReceived = null;
                Disconnected = null;

                Stop();
                acceptEventArgs.Dispose();
                while (ioEventArgsPool.TryPop(out SocketAsyncEventArgs ioEventArgs))
                {
                    ResetIOSocketEventArgs(ioEventArgs);
                    ioEventArgs.Dispose();
                }
            }
        }

        /// <summary>
        /// Start Listener
        /// </summary>
        /// <exception cref="System.InvalidOperationException" />
        /// <exception cref="System.ObjectDisposedException" />
        /// <exception cref="SocketException" />
        public void Start()
        {
            CheckDisposed();
            if (IsActived)
                throw new InvalidOperationException("actived");

            try
            {
                isInActivating = ActiveSentinel;
                var localEndPoint = listenEndPoint;
                socket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(localEndPoint);
                socket.Listen(backlog);
            }
            catch (SocketException)
            {
                Stop();
                throw;
            }

            var bufferCapacity = maxConnections * 2 * bufferSize;
            bufferManager = new BufferManager(bufferCapacity, bufferSize);
            foreach (var ioEventArgs in ioEventArgsPool)
            {
                bufferManager.SetBuffer(ioEventArgs);
            }

            PostAccept();
        }

        /// <summary>
        /// Stop Listener
        /// </summary>
        /// <exception cref="System.ObjectDisposedException" />
        public void Stop()
        {
            if (Interlocked.CompareExchange(ref isInActivating, NoneSentinel, ActiveSentinel) == ActiveSentinel)
            {
                var oldSocket = socket;
                if (oldSocket != null)
                {
                    try
                    {
                        oldSocket.Close();
                        oldSocket.Dispose();
                    }
                    catch (SocketException ex)
                    {
                        logger.Error("SocketListener Stop error:{0}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Send data
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="SocketException" />
        public void Send(ExSocket socket, byte[] data, int offset, int count)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > data.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            CheckDisposed();
            var buffer = packetProcessor.BuildPacket(data, offset, count);
            if (sendOperation == SocketOperation.Asynchronization)
            {
                if (socket.DirectSendOrEnqueue(buffer))
                {
                    if (!ioEventArgsPool.TryPop(out SocketAsyncEventArgs ioEventArgs))
                    {
                        logger.Error("apply for IoEventArgs failed...");
                        throw new SocketException((int)SocketError.NoBufferSpaceAvailable);
                    }

                    ioEventArgs.AcceptSocket = socket.WorkSocket;
                    statistics.sendConcurrencyCounter.Increment();
                    TryDequeueAndPostSend(socket, ioEventArgs);
                }
            }
            else
            {
                int length = 0;
                int influenced;
                statistics.sendConcurrencyCounter.Increment();
                while (length < buffer.Length)
                {
                    influenced = buffer.Length - length;
                    length += socket.WorkSocket.Send(buffer, length, influenced, SocketFlags.None);
                }

                statistics.sendConcurrencyCounter.Decrement();
            }
        }

        /// <summary>
        /// Send data
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <exception cref="ArgumentNullException" />
        public void Send(ExSocket socket, byte[] data, int offset)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Send(socket, data, offset, data.Length - offset);
        }

        /// <summary>
        /// Send data
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        /// <exception cref="ArgumentNullException" />
        public void Send(ExSocket socket, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Send(socket, data, 0, data.Length);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}