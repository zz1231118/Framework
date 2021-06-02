using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Framework.Log;

namespace Framework.Net.Sockets
{
    /// <summary>
    /// Async tcp client
    /// </summary>
    public class TcpClient : BaseDisposed
    {
        private const int NoneSentinel = 0;
        private const int ActiveSentinel = 1;

        private readonly byte[] lengthForBytes = new byte[sizeof(int)];
        private readonly IPacketProcessor packetProcessor;

        private static readonly ILogger logger = Logger.GetLogger<TcpClient>();
        private SocketOperation sendOperation = SocketOperation.Asynchronization;
        private SocketOperation receiveOperation = SocketOperation.Asynchronization;
        private int isInActivating;
        private bool isBlocking = true;
        private bool isConnecting;
        private int bufferSize;
        private bool noDelay;
        private KeepAlive keepAlive;
        private TimeSpan connectTimeout;
        private TimeSpan sendTimeout;
        private TimeSpan receiveTimeout;
        private ExSocket? exSocket;

        /// <inheritdoc />
        public TcpClient(IPacketProcessor packetProcessor)
        {
            if (packetProcessor == null)
                throw new ArgumentNullException(nameof(packetProcessor));

            this.packetProcessor = packetProcessor;
            this.bufferSize = SocketConstants.DefaultBufferSize;
            this.keepAlive = KeepAlive.OFF;
        }

        /// <inheritdoc />
        public TcpClient()
            : this(new DefaultPacketProcessor())
        { }

        /// <summary>
        /// IsActivated
        /// </summary>
        private bool IsActivated => isInActivating == ActiveSentinel;

        /// <summary>
        /// IsConnected
        /// </summary>
        public bool IsConnected => exSocket?.Connected == true && IsActivated;

        /// <summary>
        /// <para>IsBlocking</para>
        /// <para>default: true</para>
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ObjectDisposedException" />
        public bool IsBlocking
        {
            get => isBlocking;
            set
            {
                if (isConnecting || IsActivated)
                    throw new InvalidOperationException("actived");

                isBlocking = value;
            }
        }

        /// <summary>
        /// Local endpoint
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ObjectDisposedException" />
        public EndPoint LocalEndPoint => exSocket?.LocalEndPoint;

        /// <summary>
        /// Remote endpoint
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ObjectDisposedException" />
        public EndPoint RemoteEndPoint => exSocket?.RemoteEndPoint;

        /// <summary>
        /// <para>Buffer size</para>
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
                if (isConnecting || IsActivated)
                {
                    throw new InvalidOperationException("actived");
                }

                bufferSize = value;
            }
        }

        /// <summary>
        /// <para>NoDelay</para>
        /// <para>default: false</para>
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ObjectDisposedException" />
        public bool NoDelay
        {
            get => noDelay;
            set
            {
                CheckDisposed();
                if (isConnecting || IsActivated)
                {
                    throw new InvalidOperationException("actived");
                }

                noDelay = value;
            }
        }

        /// <summary>
        /// <para>KeepAlive</para>
        /// <para>default: off</para>
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ObjectDisposedException" />
        public KeepAlive KeepAlive
        {
            get => keepAlive;
            set
            {
                CheckDisposed();
                if (isConnecting || IsActivated)
                {
                    throw new InvalidOperationException("actived");
                }

                keepAlive = value;
            }
        }

        /// <summary>
        /// <para>Connect timeout</para>
        /// <para>default: <see cref="TimeSpan.Zero"/></para>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ObjectDisposedException" />
        public TimeSpan ConnectTimeout
        {
            get => connectTimeout;
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(nameof(value));

                CheckDisposed();
                if (isConnecting || IsActivated)
                {
                    throw new InvalidOperationException("actived");
                }

                connectTimeout = value;
            }
        }

        /// <summary>
        /// <para>Send timeout</para>
        /// <para>default: <see cref="TimeSpan.Zero"/></para>
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
                if (isConnecting || IsActivated)
                {
                    throw new InvalidOperationException("actived");
                }

                sendTimeout = value;
            }
        }

        /// <summary>
        /// <para>Receive timeout</para>
        /// <para>default: <see cref="TimeSpan.Zero"/></para>
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
                if (isConnecting || IsActivated)
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
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ObjectDisposedException" />
        public SocketOperation SendOperation
        {
            get => sendOperation;
            set
            {
                CheckDisposed();
                if (isConnecting || IsActivated)
                {
                    throw new InvalidOperationException("actived");
                }

                sendOperation = value;
            }
        }

        /// <summary>
        /// <para>Receive operation</para>
        /// <para>default: <see cref="SocketOperation.Asynchronization"/></para>
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ObjectDisposedException" />
        public SocketOperation ReceiveOperation
        {
            get => receiveOperation;
            set
            {
                CheckDisposed();
                if (isConnecting || IsActivated)
                {
                    throw new InvalidOperationException("actived");
                }

                receiveOperation = value;
            }
        }

        /// <summary>
        /// Connected event
        /// </summary>
        public event EventHandler<SocketEventArgs>? Connected;

        /// <summary>
        /// DataReceived event
        /// </summary>
        public event EventHandler<SocketEventArgs>? Received;

        /// <summary>
        /// Disconnected event
        /// </summary>
        public event EventHandler<SocketEventArgs>? Disconnected;

        private Socket CreateSocket(EndPoint endpoint)
        {
            var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = noDelay;
            socket.Blocking = isBlocking;
            if (keepAlive != KeepAlive.OFF) keepAlive.IOControl(socket);
            if (sendTimeout > TimeSpan.Zero) socket.SendTimeout = (int)sendTimeout.TotalMilliseconds;
            if (receiveTimeout > TimeSpan.Zero) socket.ReceiveTimeout = (int)receiveTimeout.TotalMilliseconds;

            return socket;
        }

        void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                ProcessConnect(e);
            }
            catch (Exception ex)
            {
                logger.Error("TcpClient Accept_Completed error:{0}", ex);
                //throw ex;
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
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
            catch (Exception ex)
            {
                logger.Error("TcpClient IO_Completed error:{0}", ex);
                //throw ex;
            }
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Connect(EndPoint endpoint)
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            CheckDisposed();
            if (IsActivated)
            {
                throw new SocketException((int)SocketError.IsConnected);
            }

            var newSocket = CreateSocket(endpoint);

            try
            {
                if (connectTimeout > TimeSpan.Zero)
                {
                    var result = newSocket.BeginConnect(endpoint, null, null);
                    if (!result.AsyncWaitHandle.WaitOne(connectTimeout))
                        throw new SocketException((int)SocketError.TimedOut);

                    newSocket.EndConnect(result);
                }
                else
                {
                    newSocket.Connect(endpoint);
                }

                Interlocked.Exchange(ref isInActivating, ActiveSentinel);
            }
            catch (Exception)
            {
                newSocket.Dispose();
                throw;
            }

            ConnectCompleted(newSocket);
        }

        /// <summary>
        /// Async connect
        /// </summary>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void ConnectAsync(EndPoint endpoint)
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            CheckDisposed();
            if (isConnecting)
            {
                throw new SocketException((int)SocketError.IOPending);
            }
            if (IsActivated)
            {
                throw new SocketException((int)SocketError.IsConnected);
            }

            isConnecting = true;
            var newSocket = CreateSocket(endpoint);
            var acceptEventArgs = new SocketAsyncEventArgs();
            acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
            acceptEventArgs.AcceptSocket = newSocket;
            acceptEventArgs.RemoteEndPoint = endpoint;
            PostConnect(acceptEventArgs);
        }

        /// <summary>
        /// Close connection
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Close()
        {
            if (IsDisposed)
                return;

            exSocket?.Close();
        }

        private void PostConnect(SocketAsyncEventArgs acceptEventArgs)
        {
            var willRaiseEvent = acceptEventArgs.AcceptSocket.ConnectAsync(acceptEventArgs);
            if (!willRaiseEvent)
            {
                ProcessConnect(acceptEventArgs);
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs acceptEventArgs)
        {
            isConnecting = false;

            try
            {
                if (acceptEventArgs.SocketError != SocketError.Success)
                {
                    acceptEventArgs.AcceptSocket.Dispose();
                    NotifyConnectedEvent(new SocketEventArgs(acceptEventArgs.SocketError));
                    return;
                }

                ConnectCompleted(acceptEventArgs.AcceptSocket);
            }
            finally
            {
                acceptEventArgs.Dispose();
            }
        }

        private void ConnectCompleted(Socket workSocket)
        {
            var newSocket = new ExSocket(workSocket);
            if (sendOperation == SocketOperation.Asynchronization || receiveOperation == SocketOperation.Asynchronization)
            {
                var dataToken = new DataToken();
                dataToken.Socket = newSocket;

                var buffer = new byte[bufferSize];
                var sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                sendEventArgs.UserToken = dataToken;
                sendEventArgs.AcceptSocket = newSocket.WorkSocket;
                sendEventArgs.SetBuffer(buffer, 0, buffer.Length);

                dataToken = new DataToken();
                dataToken.Socket = newSocket;
                buffer = new byte[bufferSize];
                var recvEventArgs = new SocketAsyncEventArgs();
                recvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                recvEventArgs.UserToken = dataToken;
                recvEventArgs.SetBuffer(buffer, 0, buffer.Length);
                recvEventArgs.AcceptSocket = newSocket.WorkSocket;

                var context = new Context(sendEventArgs, recvEventArgs);
                newSocket.UserToken = context;
            }

            exSocket = newSocket;
            Interlocked.Exchange(ref isInActivating, ActiveSentinel);
            NotifyConnectedEvent(new SocketEventArgs(newSocket, SocketError.Success));
            if (newSocket.IsClosed) DoClosed(newSocket, SocketError.OperationAborted);
            else if (receiveOperation == SocketOperation.Asynchronization) PostReceive(((Context)newSocket.UserToken).recvEventArgs);
        }

        private void PostReceive(SocketAsyncEventArgs ioEventArgs)
        {
            var willRaiseEvent = ioEventArgs.AcceptSocket.ReceiveAsync(ioEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(ioEventArgs);
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs ioEventArgs)
        {
            var dataToken = (DataToken)ioEventArgs.UserToken;
            var exSocket = dataToken.Socket;
            if (ioEventArgs.SocketError == SocketError.OperationAborted)
            {
                DoClosed(exSocket, ioEventArgs.SocketError);
                return;
            }
            if (ioEventArgs.SocketError != SocketError.Success)
            {
                DoClosing(exSocket, ioEventArgs.SocketError);
                return;
            }
            if (ioEventArgs.BytesTransferred == 0)
            {
                DoClosing(exSocket, SocketError.ConnectionReset);
                return;
            }

            var packetStreamer = exSocket.PacketStreamer;
            if (!packetProcessor.HandlePacket(ioEventArgs, packetStreamer))
            {
                DoClosing(exSocket, SocketError.SocketError);
                return;
            }
            while (packetStreamer.TryDequeue(out byte[] bytes))
            {
                NotifyReceivedEvent(new SocketEventArgs(exSocket, SocketError.Success, bytes));
            }
            if (exSocket.IsClosed)
            {
                DoClosed(exSocket, SocketError.OperationAborted);
                return;
            }

            try
            {
                PostReceive(ioEventArgs);
            }
            catch (ObjectDisposedException)
            {
                DoClosed(exSocket, SocketError.OperationAborted);
            }
        }

        /// <summary>
        /// Send
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Send(byte[] data, int offset, int count)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset >= data.Length)
                throw new ArgumentNullException(nameof(offset));
            if (count < 0 || offset + count > data.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            CheckDisposed();
            if (!IsActivated)
            {
                throw new SocketException((int)SocketError.NotConnected);
            }
            var socket = exSocket;
            if (socket == null)
            {
                throw new SocketException((int)SocketError.OperationAborted);
            }
            if (sendOperation == SocketOperation.Synchronization)
            {
                int length = 0;
                int influenced;
                var buffer = packetProcessor.BuildPacket(data, offset, count);
                while (length < buffer.Length)
                {
                    influenced = buffer.Length - length;
                    length += socket.WorkSocket.Send(buffer, length, influenced, SocketFlags.None);
                }
            }
            else
            {
                var buffer = packetProcessor.BuildPacket(data, offset, count);
                var context = (Context)socket.UserToken;
                if (socket.DirectSendOrEnqueue(buffer))
                {
                    TryDequeueAndPostSend(socket, context.sendEventArgs);
                }
            }
        }

        /// <summary>
        /// Send
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public void Send(byte[] data, int offset)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Send(data, offset, data.Length - offset);
        }

        /// <summary>
        /// Send
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Send(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Send(data, 0, data.Length);
        }

        /// <summary>
        /// Receive
        /// </summary>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] Receive()
        {
            CheckDisposed();
            if (!IsActivated)
            {
                throw new SocketException((int)SocketError.NotConnected);
            }
            if (receiveOperation != SocketOperation.Synchronization)
            {
                throw new SocketException((int)SocketError.OperationNotSupported);
            }

            Array.Clear(lengthForBytes, 0, lengthForBytes.Length);

            int count;
            int offset = 0;
            var workSocket = exSocket.WorkSocket;
            while (offset < lengthForBytes.Length)
            {
                count = lengthForBytes.Length - offset;
                offset += workSocket.Receive(lengthForBytes, offset, count, SocketFlags.None);
            }
            var length = BitConverter.ToInt32(lengthForBytes, 0);
            var buffer = new byte[length];
            offset = 0;
            while (offset < buffer.Length)
            {
                count = buffer.Length - offset;
                offset += workSocket.Receive(buffer, offset, count, SocketFlags.None);
            }

            NotifyReceivedEvent(new SocketEventArgs(exSocket, SocketError.Success, buffer));
            return buffer;
        }

        private void TryDequeueAndPostSend(ExSocket socket, SocketAsyncEventArgs ioEventArgs)
        {
            if (socket.TryDequeueOrReset(out byte[] data))
            {
                var dataToken = (DataToken)ioEventArgs.UserToken;
                dataToken.Socket = socket;
                dataToken.ByteArrayForMessage = data;
                dataToken.MessageLength = data.Length;

                PostSend(ioEventArgs);
            }
        }

        private void PostSend(SocketAsyncEventArgs ioEventArgs)
        {
            var dataToken = (DataToken)ioEventArgs.UserToken;
            var copyedBytes = Math.Min(bufferSize, dataToken.MessageLength - dataToken.MessageBytesDone);
            ioEventArgs.SetBuffer(ioEventArgs.Offset, copyedBytes);
            Buffer.BlockCopy(dataToken.ByteArrayForMessage, dataToken.MessageBytesDone, ioEventArgs.Buffer, ioEventArgs.Offset, copyedBytes);

            bool willRaiseEvent;
            try
            {
                willRaiseEvent = ioEventArgs.AcceptSocket.SendAsync(ioEventArgs);
            }
            catch (ObjectDisposedException)
            {
                //LogManager.Error.Log("TcpClient PostSend error:{0}", ex);
                return;
            }
            if (!willRaiseEvent)
            {
                ProcessSend(ioEventArgs);
            }
        }

        private void ProcessSend(SocketAsyncEventArgs ioEventArgs)
        {
            var dataToken = (DataToken)ioEventArgs.UserToken;
            if (ioEventArgs.SocketError == SocketError.Success)
            {
                //异常全部放在接收逻辑中处理，发送逻辑忽略
                dataToken.MessageBytesDone += ioEventArgs.BytesTransferred;
                if (dataToken.MessageBytesDone < dataToken.MessageLength)
                {
                    PostSend(ioEventArgs);
                }
                else
                {
                    dataToken.Reset();
                    TryDequeueAndPostSend(dataToken.Socket, ioEventArgs);
                }
            }
        }

        private void DoClosing(ExSocket exSocket, SocketError socketError)
        {
            exSocket.Close();
            DoClosed(exSocket, socketError);
        }

        private void DoClosed(ExSocket exSocket, SocketError socketError)
        {
            if (Interlocked.CompareExchange(ref isInActivating, NoneSentinel, ActiveSentinel) == ActiveSentinel)
            {
                if (exSocket.UserToken is Context context) context.Dispose();
                NotifyDisconnectedEvent(new SocketEventArgs(exSocket, socketError));
            }
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
                    logger.Error("TcpClient Connected event error:{0}", ex);
                }
            }
        }

        private void NotifyReceivedEvent(SocketEventArgs e)
        {
            var received = Received;
            if (received != null)
            {
                try
                {
                    received(this, e);
                }
                catch (Exception ex)
                {
                    logger.Error("TcpClient Received event error:{0}", ex);
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
                    logger.Error("TcpClient Disconnected event error:{0}", ex);
                }
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                try
                {
                    Connected = null;
                    Received = null;
                    Disconnected = null;

                    exSocket?.Close();
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }

        class Context : BaseDisposed
        {
            public readonly SocketAsyncEventArgs sendEventArgs;
            public readonly SocketAsyncEventArgs recvEventArgs;

            public Context(SocketAsyncEventArgs sendEventArgs, SocketAsyncEventArgs recvEventArgs)
            {
                this.sendEventArgs = sendEventArgs;
                this.recvEventArgs = recvEventArgs;
            }

            private void ReleaseIOEventArgs(SocketAsyncEventArgs ioEventArgs)
            {
                var dataToken = (DataToken)ioEventArgs.UserToken;
                dataToken.Socket = null;
                dataToken.Reset();
                ioEventArgs.AcceptSocket = null;
            }

            protected override void Dispose(bool disposing)
            {
                if (!IsDisposed)
                {
                    try
                    {
                        ReleaseIOEventArgs(sendEventArgs);
                        ReleaseIOEventArgs(recvEventArgs);

                        sendEventArgs.Dispose();
                        recvEventArgs.Dispose();
                    }
                    finally
                    {
                        base.Dispose(disposing);
                    }
                }
            }
        }
    }
}