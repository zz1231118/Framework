using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Framework.Log;
using Framework.Net.Sockets;

namespace Framework.Net.WebSockets
{
    public class WebSocketListener : BaseDisposed
    {
        private readonly ILogger logger = Logger.GetLogger<WebSocketListener>();
        private readonly WebSocketListenerSetting setting;
        private readonly BlockingCollection<SocketAsyncEventArgs> acceptEventArgsPool;
        private readonly ConcurrentStack<SocketAsyncEventArgs> ioEventArgsPool;
        private readonly BufferManager bufferManager;
        private readonly ConcurrentQueue<WebSocket> openHandshakePendingQueue = new ConcurrentQueue<WebSocket>();
        private readonly ConcurrentQueue<WebSocket> closeHandshakePendingQueue = new ConcurrentQueue<WebSocket>();
        private readonly HandshakeProcessor handshakeProcessor;
        private readonly MessageProcessor messageProcessor;
        private Socket socket;
        private Timer handshakePendingQueueCheckingTimer;
        private bool isActivated;

        public WebSocketListener(WebSocketListenerSetting setting)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            this.setting = setting;
            if (setting.Version >= 13)
            {
                handshakeProcessor = new Rfc6455HandshakeProcessor(this, setting.Encoding);
                messageProcessor = new Rfc6455MessageProcessor();
            }
            else if (setting.Version >= 8)
            {
                handshakeProcessor = new Hybi10HandshakeProcessor(this, setting.Encoding);
                messageProcessor = new Hybi10MessageProcessor();
            }
            else
            {
                handshakeProcessor = new Hybi00HandshakeProcessor(this, setting.Encoding);
                messageProcessor = new Hybi00MessageProcessor();
            }
            for (int i = 0; i < setting.MaxAcceptOps; i++)
            {
                var acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
                acceptEventArgsPool.Add(acceptEventArgs);
            }
            var numOfSaeaForRecSend = setting.MaxConnections * 2;
            bufferManager = new BufferManager(numOfSaeaForRecSend, setting.BufferSize);
            ioEventArgsPool = new ConcurrentStack<SocketAsyncEventArgs>();
            for (int i = 0; i < numOfSaeaForRecSend; i++)
            {
                var ioEventArgs = new SocketAsyncEventArgs();
                ioEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                bufferManager.SetBuffer(ioEventArgs);

                var dataToken = new WSDataToken();
                ioEventArgs.UserToken = dataToken;
                ioEventArgsPool.Push(ioEventArgs);
            }
        }

        public WebSocketListenerSetting Setting => setting;
        public int Version => setting.Version;
        public bool IsSecurity => setting.IsSecurity;
        public bool IsActivated => isActivated;

        public event EventHandler<WebSocketEventArgs> Ping;
        public event EventHandler<WebSocketEventArgs> Pong;
        public event EventHandler<WebSocketEventArgs> Connected;
        public event EventHandler<WebSocketEventArgs> DataReceived;
        public event EventHandler<WebSocketEventArgs> Disconnected;

        public void Start()
        {
            CheckDisposed();
            if (isActivated)
                throw new InvalidOperationException("actived");

            try
            {
                isActivated = true;
                var localEndPoint = setting.EndPoint;
                socket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(localEndPoint);
                socket.Listen(setting.Backlog);
            }
            catch (SocketException)
            {
                Close();
                throw;
            }

            PostAccept();

            var checkingInterval = setting.HandshakePendingQueueCheckingInterval;
            handshakePendingQueueCheckingTimer = new Timer(new TimerCallback(HandshakePendingQueueCheckingCallbacked), null, checkingInterval, checkingInterval);
        }

        public void Close()
        {
            if (isActivated)
            {
                try
                {
                    socket.Dispose();
                    handshakePendingQueueCheckingTimer.Dispose();

                    socket = null;
                    handshakePendingQueueCheckingTimer = null;
                }
                finally
                {
                    isActivated = false;
                }
            }
        }

        private void PostAccept()
        {
            if (!IsActivated)
            {
                //not start
                return;
            }

            var acceptEventArgs = acceptEventArgsPool.Take();
            bool willRaiseEvent;
            try
            {
                willRaiseEvent = socket.AcceptAsync(acceptEventArgs);
            }
            catch (ObjectDisposedException)
            {
                acceptEventArgsPool.Add(acceptEventArgs);
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
                if (acceptEventArgs.SocketError != SocketError.Success)
                {
                    ResetSAEAObject(acceptEventArgs);
                    return;
                }
                if (!ioEventArgsPool.TryPop(out SocketAsyncEventArgs ioEventArgs))
                {
                    ResetSAEAObject(acceptEventArgs);
                    logger.Error("apply for IoEventArgs failed...");
                    return;
                }

                ioEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;
                acceptEventArgs.AcceptSocket = null;
                acceptEventArgsPool.Add(acceptEventArgs);

                var cltSocket = ioEventArgs.AcceptSocket;
                var newSocket = new WebSocket(this, cltSocket);
                newSocket.LastAccessTime = DateTime.Now;

                var dataToken = (DataToken)ioEventArgs.UserToken;
                dataToken.Socket = newSocket;
                dataToken.Reset();
                ioEventArgs.SetBuffer(ioEventArgs.Offset, setting.BufferSize);
                openHandshakePendingQueue.Enqueue(newSocket);

                PostReceive(ioEventArgs);
            }
            finally
            {
                PostAccept();
            }
        }

        internal void PostSend(WebSocket socket, sbyte opcode, byte[] data, int offset, int count)
        {
            byte[] buffer = messageProcessor.BuildMessagePack(socket, opcode, data, offset, count);
            SendAsync(socket, buffer);
        }

        internal bool SendAsync(ExSocket socket, byte[] buffer)
        {
            if (socket.DirectSendOrEnqueue(buffer))
            {
                TryDequeueAndPostSend(socket, null);
                return true;
            }
            return false;
        }

        internal void Closing(SocketAsyncEventArgs ioEventArgs, sbyte opCode = Opcode.Close, string reason = "")
        {
            bool needClose = true;
            var dataToken = (WSDataToken)ioEventArgs.UserToken;
            var socket = (WebSocket)dataToken.Socket;
            try
            {
                if (opCode != Opcode.Empty)
                {
                    byte[] data = messageProcessor.CloseMessage(socket, opCode, reason);
                    if (data != null) SendAsync(socket, data);
                }
                if (ioEventArgs.AcceptSocket != null)
                {
                    try
                    {
                        ioEventArgs.AcceptSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    { }
                }
            }
            catch (Exception ex)
            {
                logger.Error("closing error:{0}", ex);
                needClose = false;
            }
            if (needClose)
            {
                try
                {
                    Disconnected?.Invoke(this, new WebSocketEventArgs(socket));
                }
                catch (Exception ex)
                {
                    logger.Error("OnDisconnected error:{0}", ex);
                }
                ResetSAEAObject(ioEventArgs);
            }
            ReleaseIOEventArgs(ioEventArgs);
        }

        void HandshakePendingQueueCheckingCallbacked(object obj)
        {
            try
            {
                handshakePendingQueueCheckingTimer.Change(Timeout.Infinite, Timeout.Infinite);

                WebSocket session;
                while (true)
                {
                    if (!openHandshakePendingQueue.TryPeek(out session))
                        break;

                    if (session.Handshake.Handshaked || !session.Connected)
                    {
                        openHandshakePendingQueue.TryDequeue(out session);
                        continue;
                    }

                    if (DateTime.Now < session.ConnectionTime + setting.OpenHandshakeTimeout)
                        break;

                    openHandshakePendingQueue.TryDequeue(out session);
                    session.Close();
                }

                while (true)
                {
                    if (!closeHandshakePendingQueue.TryPeek(out session))
                        break;

                    if (!session.Connected)
                    {
                        closeHandshakePendingQueue.TryDequeue(out session);
                        continue;
                    }

                    if (DateTime.Now < session.StartCloseTime + setting.CloseHandshakeTimeout)
                        break;

                    closeHandshakePendingQueue.TryDequeue(out session);
                    session.Close();
                }
            }
            catch (Exception)
            { }
            finally
            {
                var checkingInterval = setting.HandshakePendingQueueCheckingInterval;
                handshakePendingQueueCheckingTimer.Change(checkingInterval, checkingInterval);
            }
        }

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

        void IO_Completed(object sender, SocketAsyncEventArgs ioEventArgs)
        {
            var dataToken = (WSDataToken)ioEventArgs.UserToken;
            try
            {
                switch (ioEventArgs.LastOperation)
                {
                    case SocketAsyncOperation.Receive:
                        ProcessReceive(ioEventArgs);
                        break;
                    case SocketAsyncOperation.Send:
                        ProcessSend(ioEventArgs);
                        break;

                    default:
                        throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                }
            }
            catch (ObjectDisposedException)
            {
                ReleaseIOEventArgs(ioEventArgs);
            }
            catch (Exception ex)
            {
                logger.Error("IP {0} IO_Completed unkown error:{1}", dataToken?.Socket?.RemoteEndPoint, ex);
            }
        }

        private void SocketListener_SocketConnected(object sender, SocketEventArgs e)
        {
            var session = (WebSocket)e.Socket;
            if (!ioEventArgsPool.TryPop(out SocketAsyncEventArgs ioEventArgs))
            {
                session.Close();
                return;
            }

            openHandshakePendingQueue.Enqueue(session);

            var dataToken = (WSDataToken)ioEventArgs.UserToken;
            dataToken.Socket = session;
            dataToken.Reset();

            ioEventArgs.AcceptSocket = session.WorkSocket;
            bufferManager.SetBuffer(ioEventArgs);

            PostReceive(ioEventArgs);
        }

        private void PostReceive(SocketAsyncEventArgs ioEventArgs)
        {
            bool willRaiseEvent = ioEventArgs.AcceptSocket.ReceiveAsync(ioEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(ioEventArgs);
            }
        }

        private bool TryReceiveMessage(SocketAsyncEventArgs ioEventArgs, out List<DataMessage> messages, out bool hasHandshaked)
        {
            messages = new List<DataMessage>();
            hasHandshaked = false;
            try
            {
                var dataToken = (WSDataToken)ioEventArgs.UserToken;
                var socket = (WebSocket)dataToken.Socket;
                var buffer = new byte[ioEventArgs.BytesTransferred];
                Buffer.BlockCopy(ioEventArgs.Buffer, ioEventArgs.Offset, buffer, 0, buffer.Length);
                if (socket.Handshake == null)
                {
                    socket.Handshake = new HandshakeData();
                }
                if (!socket.Handshake.Handshaked)
                {
                    var result = handshakeProcessor.Receive(ioEventArgs, dataToken, buffer);
                    if (result == HandshakeResult.Success)
                    {
                        hasHandshaked = true;
                    }
                    else if (result == HandshakeResult.Close)
                    {
                        Closing(ioEventArgs, Opcode.Close, "receive handshake fail");
                        return false;
                    }
                    return true;
                }
                if (messageProcessor != null)
                {
                    messageProcessor.TryReadMeaage(dataToken, buffer, out messages);
                }
                if (dataToken.HeadFrame != null && !dataToken.HeadFrame.CheckRSV)
                {
                    Closing(ioEventArgs, Opcode.Close, "receive data RSV error");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("TryReceiveMessage error:{0}", ex);
            }
            return true;
        }

        private void ProcessReceive(SocketAsyncEventArgs ioEventArgs)
        {
            var dataToken = (WSDataToken)ioEventArgs.UserToken;
            var socket = (WebSocket)dataToken.Socket;
            if (ioEventArgs.BytesTransferred == 0)
            {
                //对方主动关闭socket
                Closing(ioEventArgs, Opcode.Empty);
                return;
            }
            if (ioEventArgs.SocketError != SocketError.Success)
            {
                //Socket错误
                Closing(ioEventArgs);
                return;
            }

            bool needPostAnother = TryReceiveMessage(ioEventArgs, out List<DataMessage> messages, out bool hasHandshaked);
            if (hasHandshaked)
            {
                Connected?.Invoke(this, new WebSocketEventArgs(socket));
            }
            if (messages != null)
            {
                foreach (var message in messages)
                {
                    try
                    {
                        switch (message.Opcode)
                        {
                            case Opcode.Close:
                                Closing(ioEventArgs);
                                needPostAnother = false;
                                break;
                            case Opcode.Ping:
                                Ping?.Invoke(this, new WebSocketEventArgs(socket, message));
                                break;
                            case Opcode.Pong:
                                Pong?.Invoke(this, new WebSocketEventArgs(socket, message));
                                break;
                            default:
                                DataReceived?.Invoke(this, new WebSocketEventArgs(socket, message));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("OnDataReceived error:{0}", ex);
                    }
                }
            }
            if (needPostAnother)
            {
                if (socket.IsClosed)
                {
                    ResetSAEAObject(ioEventArgs);
                }
                else
                {
                    PostReceive(ioEventArgs);
                }
            }
        }

        private void TryDequeueAndPostSend(ExSocket socket, SocketAsyncEventArgs ioEventArgs)
        {
            bool isOwner = ioEventArgs == null;
            if (socket.TryDequeueOrReset(out byte[] data))
            {
                if (ioEventArgs == null)
                {
                    ioEventArgsPool.TryPop(out ioEventArgs);
                    ioEventArgs.AcceptSocket = socket.WorkSocket;
                    bufferManager.SetBuffer(ioEventArgs);
                }

                var dataToken = (DataToken)ioEventArgs.UserToken;
                dataToken.Socket = socket;
                dataToken.byteArrayForMessage = data;
                dataToken.messageLength = data.Length;

                try
                {
                    PostSend(ioEventArgs);
                }
                catch (Exception)
                {
                    //dataToken.ResultCallback(ResultCode.Error, ex);
                    if (isOwner)
                    {
                        ReleaseIOEventArgs(ioEventArgs);
                    }
                    //socket.ResetSendFlag();
                }
            }
            else
            {
                ReleaseIOEventArgs(ioEventArgs);
                //socket.ResetSendFlag();
            }
        }

        private void PostSend(SocketAsyncEventArgs ioEventArgs)
        {
            var dataToken = (DataToken)ioEventArgs.UserToken;
            if (dataToken.messageLength - dataToken.messageBytesDone <= setting.BufferSize)
            {
                ioEventArgs.SetBuffer(ioEventArgs.Offset, dataToken.messageLength - dataToken.messageBytesDone);
                Buffer.BlockCopy(dataToken.byteArrayForMessage, dataToken.messageBytesDone, ioEventArgs.Buffer, ioEventArgs.Offset, dataToken.messageLength - dataToken.messageBytesDone);
            }
            else
            {
                ioEventArgs.SetBuffer(ioEventArgs.Offset, setting.BufferSize);
                Buffer.BlockCopy(dataToken.byteArrayForMessage, dataToken.messageBytesDone, ioEventArgs.Buffer, ioEventArgs.Offset, setting.BufferSize);
            }

            var willRaiseEvent = ioEventArgs.AcceptSocket.SendAsync(ioEventArgs);
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
                dataToken.messageBytesDone += ioEventArgs.BytesTransferred;
                if (dataToken.messageBytesDone != dataToken.messageLength)
                {
                    PostSend(ioEventArgs);
                }
                else
                {
                    //dataToken.ResultCallback(ResultCode.Success);
                    dataToken.Reset();
                    try
                    {
                        TryDequeueAndPostSend(dataToken.Socket, ioEventArgs);
                    }
                    catch
                    {
                        //dataToken.Socket.ResetSendFlag();
                        throw;
                    }
                }
            }
            else
            {
                //dataToken.ResultCallback(ResultCode.Close);
                //dataToken.Socket.ResetSendFlag();
                Closing(ioEventArgs, Opcode.Empty);
            }
        }

        private void ReleaseIOEventArgs(SocketAsyncEventArgs ioEventArgs)
        {
            var dataToken = (DataToken)ioEventArgs.UserToken;
            dataToken.Socket = null;
            dataToken.Reset();
            ioEventArgs.AcceptSocket = null;

            bufferManager.FreeBuffer(ioEventArgs);
            ioEventArgsPool.Push(ioEventArgs);
        }

        private static void ResetSAEAObject(SocketAsyncEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.AcceptSocket != null)
                {
                    eventArgs.AcceptSocket.Close();
                }
            }
            catch (Exception)
            { }

            eventArgs.AcceptSocket = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                try
                {
                    Close();
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }
    }
}
