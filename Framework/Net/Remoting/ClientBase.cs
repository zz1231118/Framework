using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Framework.JavaScript;
using Framework.Log;
using Framework.Net.Remoting.Packets;
using Framework.Net.Sockets;
using Framework.Security;

namespace Framework.Net.Remoting
{
    public abstract class ClientBase : BaseDisposed
    {
        private static readonly ILogger logger = Logger.GetLogger<ClientBase>();
        private static readonly ConcurrentDictionary<Type, ClientTypeBuilder> _kvTypeBuilder = new ConcurrentDictionary<Type, ClientTypeBuilder>();
        private static readonly Func<Type, ClientTypeBuilder> _kvTypeBuilderFactory = key => new ClientTypeBuilder(key);
        private readonly ConcurrentDictionary<long, Request> _kvRequest = new ConcurrentDictionary<long, Request>();
        private readonly ConcurrentDictionary<MethodBase, OperationContractAttribute> _kvOperContract = new ConcurrentDictionary<MethodBase, OperationContractAttribute>();
        private readonly Func<MethodBase, OperationContractAttribute> _operContractFactory = key => key.GetCustomAttribute<OperationContractAttribute>();
        private bool _active;
        private TcpClient _tcpClient;
        private ClientEndpoint _endpoint;
        private IClientCredentials? _credentials;

        public ClientBase(ClientEndpoint endpoint)
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            _endpoint = endpoint;
        }

        ~ClientBase()
        {
            Dispose(false);
        }

        public bool Active => _active;

        public ClientEndpoint Endpoint => _endpoint;

        public IClientCredentials? Credentials
        {
            get { return _credentials; }
            set
            {
                CheckDisposed();
                if (Active)
                    throw new InvalidOperationException("active");

                _credentials = value;
            }
        }

        public DateTime LastActiveTime { get; private set; }

        public event EventHandler? Connected;

        public event EventHandler? Disconnected;

        /// <summary>
        /// Create dynamic client
        /// </summary>
        /// <param name="clientType">client interface type</param>
        /// <param name="endpoint">server remote endpoint</param>
        /// <param name="credentials">credentials</param>
        /// <returns></returns>
        public static object Create(Type clientType, ClientEndpoint endpoint, IClientCredentials? credentials = null)
        {
            if (clientType == null)
                throw new ArgumentNullException(nameof(clientType));
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));
            if (!clientType.IsInterface)
                throw new ArgumentException("T must be an interface");

            var factory = _kvTypeBuilder.GetOrAdd(clientType, _kvTypeBuilderFactory);
            var client = factory.Create(endpoint);
            client._credentials = credentials;
            return client;
        }

        /// <summary>
        /// Create dynamic client
        /// </summary>
        /// <typeparam name="T">client interface type</typeparam>
        /// <param name="endpoint">server remote endpoint</param>
        /// <param name="credentials">credentials</param>
        /// <returns></returns>
        public static T Create<T>(ClientEndpoint endpoint, IClientCredentials? credentials = null)
            where T : class
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));
            if (!typeof(T).IsInterface)
                throw new ArgumentException("T must be an interface");

            var factory = _kvTypeBuilder.GetOrAdd(typeof(T), _kvTypeBuilderFactory);
            var client = factory.Create(endpoint);
            client._credentials = credentials;
            return (T)(object)client;
        }

        /// <summary>
        /// Close proxy
        /// </summary>
        /// <param name="proxy">proxy</param>
        public static void Close(object proxy)
        {
            if (proxy == null)
                throw new ArgumentNullException(nameof(proxy));
            if (proxy is not ClientBase client)
                throw new ArgumentException(nameof(proxy));

            client.Close();
        }

        /// <summary>
        /// Open
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Open()
        {
            CheckDisposed();
            if (_active)
                throw new InvalidOperationException("active is true!");

            _active = true;
            _tcpClient = new TcpClient();
            _tcpClient.NoDelay = _endpoint.NoDelay;
            _tcpClient.KeepAlive = _endpoint.KeepAlive;
            _tcpClient.Connected += TcpClient_Connected;
            _tcpClient.Received += TcpClient_Received;
            _tcpClient.Disconnected += TcpClient_Disconnected;

            try
            {
                _tcpClient.Connect(_endpoint.EndPoint);
            }
            catch (Exception)
            {
                DoClosed();
                throw;
            }
        }

        /// <summary>
        /// Close
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Close()
        {
            if (!IsDisposed)
            {
                if (_active)
                {
                    DoClosed();
                }
            }
        }

        void TcpClient_Connected(object sender, SocketEventArgs e)
        {
            if (e.SocketError != System.Net.Sockets.SocketError.Success)
                return;

            LastActiveTime = DateTime.Now;
            RaiseConnectedEvent(EventArgs.Empty);
            if (Credentials != null)
            {
                var json = JsonSerializer.Serialize(Credentials.Create());
                var package = new Packet(ActionType.Validate, MethodType.Json, json);
                PostSend(package);
            }
        }

        void TcpClient_Received(object sender, SocketEventArgs e)
        {
            try
            {
                LastActiveTime = DateTime.Now;
                var packet = PacketFactory.Create(e.Data);
                switch (packet.Version)
                {
                    case 2:
                        Packet2Handler(packet as Packet);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error("ClientBase DataReceived error:{0}", ex);
            }
        }

        void TcpClient_Disconnected(object sender, SocketEventArgs e)
        {
            DoClosed();
            RaiseDisconnectedEvent(EventArgs.Empty);
        }

        private void Packet2Handler(Packet packet)
        {
            switch (packet.Action)
            {
                case ActionType.Heartbeat:
                    PostSend(new Packet(ActionType.Heartbeat, MethodType.Json, null));
                    break;
                case ActionType.Error:
                    {
                        var response = new Response() { Status = ResponseStatus.Error, Result = packet.Json };
                        foreach (var request in _kvRequest.Values)
                            request.With(response);

                        _kvRequest.Clear();
                    }
                    break;
                case ActionType.Request:
                    break;
                case ActionType.Response:
                    {
                        var response = JsonSerializer.Deserialize<Response>(packet.Json);
                        if (_kvRequest.TryGetValue(response.Id, out Request request))
                        {
                            request.With(response);
                        }
                    }
                    break;
                default:
                    throw new Exception("ActionType: " + packet.Action.ToString() + " error!");
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void FlushConnected()
        {
            if (!Active)
            {
                Open();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void DoClosed()
        {
            _active = false;
            _kvRequest.Clear();

            _tcpClient.Close();
            _tcpClient.Dispose();
            _tcpClient = null;
        }

        private void PostSend(IPacket package)
        {
            var byteAryForMsg = package.Data;
            _tcpClient.Send(byteAryForMsg, 0, byteAryForMsg.Length);
        }

        private Request CreateRequest(string command, object[] args)
        {
            var request = new Request();
            request.Command = command;
            if (args != null && args.Length > 0)
            {
                request.Args = new JsonArray();
                foreach (var arg in args)
                {
                    var json = JsonSerializer.Serialize(arg);
                    request.Args.Add(json);
                }
            }
            return request;
        }

        private Response SendRequest(MethodBase method, params object[] args)
        {
            FlushConnected();

            var request = CreateRequest(method.Name, args);
            var operationContract = _kvOperContract.GetOrAdd(method, _operContractFactory);
            if (operationContract != null)
                request.Method = operationContract.Method;

            _kvRequest[request.Id] = request;
            try
            {
                var json = JsonSerializer.Serialize(request);
                var package = new Packet(ActionType.Request, request.Method, json);
                PostSend(package);

                if (!request.Wait(_endpoint.Timeout))
                    throw new TimeoutException("request timeout!");

                var response = request.Response;
                if (response.Status != ResponseStatus.Success)
                    throw new RequestException(response.GetResult<string>());

                return response;
            }
            finally
            {
                if (_kvRequest.TryRemove(request.Id, out request))
                    request.Dispose();
            }
        }

        /// <summary>
        /// Request
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.TimeoutException"></exception>
        /// <exception cref="Framework.Net.Remoting.RequestException"></exception>
        protected void Request(MethodBase method, params object[] args)
        {
            CheckDisposed();
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            SendRequest(method, args);
        }

        /// <summary>
        /// Request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.TimeoutException"></exception>
        /// <exception cref="Framework.Net.Remoting.RequestException"></exception>
        /// <returns></returns>
        protected T Request<T>(MethodBase method, params object[] args)
        {
            CheckDisposed();
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            return SendRequest(method, args).GetResult<T>();
        }

        private void RaiseConnectedEvent(EventArgs e)
        {
            var connected = Connected;
            if (connected != null)
            {
                try
                {
                    connected(this, e);
                }
                catch (Exception ex)
                {
                    logger.Error("ClientBase Connected event error:{0}", ex);
                }
            }
        }

        private void RaiseDisconnectedEvent(EventArgs e)
        {
            var disconnected = Disconnected;
            if (disconnected != null)
            {
                try
                {
                    disconnected(this, e);
                }
                catch (Exception ex)
                {
                    logger.Error("ClientBase Disconnected event error:{0}", ex);
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                try
                {
                    Connected = null;
                    Disconnected = null;

                    if (Active)
                    {
                        DoClosed();
                    }
                    if (disposing)
                    {
                        _endpoint = null;
                    }
                }
                catch
                {
                    base.Dispose(disposing);
                }
            }
        }
    }
}