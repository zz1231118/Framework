using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Framework.Log;
using Framework.Net.Remoting.App;
using Framework.Net.Remoting.App.Impl;
using Framework.Net.Remoting.Packets;
using Framework.Net.Sockets;
using Framework.Security;

namespace Framework.Net.Remoting
{
    public class ServiceHost : BaseDisposed
    {
        private static readonly ILogger logger = Logger.GetLogger<ServiceHost>();
        private Func<object> _cmdFactory;
        private ServiceSetting _setting;
        private bool m_Active;
        private SocketListener _socketListen;
        private ServiceContractAttribute _serviceContract;
        private IAppListener _appListener;
        private ConcurrentDictionary<Guid, IClientSessionListener> _kvListener;
        private IServerCredentials _credentials;
        private HostContext _hostContext;
        private Timer _timer;

        public ServiceHost(object singleInstance, ServiceSetting setting)
        {
            if (singleInstance == null)
                throw new ArgumentNullException(nameof(singleInstance));
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            _cmdFactory = () => singleInstance;
            _setting = setting;

            Init(singleInstance.GetType());
        }
        public ServiceHost(Type serviceType, ServiceSetting setting)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            _cmdFactory = () => Activator.CreateInstance(serviceType);
            _setting = setting;

            Init(serviceType);
        }

        public ServiceSetting Setting => _setting;
        public bool Active => m_Active;
        public IServerCredentials Credentials
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

        public event EventHandler<SessionEventArgs> NewerSession;
        public event EventHandler<SessionEventArgs> Disconnected;

        private void Init(Type serviceType)
        {
            _serviceContract = serviceType.GetCustomAttribute<ServiceContractAttribute>();
            if (_serviceContract == null)
                throw new ArgumentException("serviceType not exists ServiceContractAttribute");

            _kvListener = new ConcurrentDictionary<Guid, IClientSessionListener>();
            _appListener = new AppListenerImpl();
            _appListener.Initialize();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Open()
        {
            CheckDisposed();
            if (m_Active)
                throw new InvalidOperationException("active is true!");

            m_Active = true;

            _hostContext = new HostContext();
            _hostContext.ServiceContract = _serviceContract;
            _hostContext.Credentials = _credentials;

            var endpoint = _setting.Endpoint;
            _socketListen = new SocketListener(endpoint.EndPoint, endpoint.Backlog, endpoint.MaxConnection);
            _socketListen.NoDelay = endpoint.NoDelay;
            _socketListen.KeepAlive = endpoint.KeepAlive;
            _socketListen.Connected += SocketListen_Connected;
            _socketListen.DataReceived += SocketListen_DataReceived;
            _socketListen.Disconnected += SocketListen_Disconnected;
            _socketListen.Start();

            if (_setting.SessionCheckInterval > TimeSpan.Zero)
                _timer = new Timer(new TimerCallback(ClearSession), null, _setting.SessionCheckInterval, _setting.SessionCheckInterval);
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Close()
        {
            CheckDisposed();

            if (!m_Active)
                throw new InvalidOperationException("active is false!");

            DoClosed();
        }

        void SocketListen_Connected(object sender, SocketEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
                return;

            try
            {
                var command = _cmdFactory.Invoke();
                var session = new ClientSessionImpl(_socketListen, _hostContext, e.Socket, command);
                var listener = _appListener.LoggedIn(session);
                if (listener == null)
                {
                    session.Close();
                    return;
                }

                _kvListener[session.HashCode] = listener;
                SessionContext.Session = session;
                try
                {
                    RaiseNewerSessionEvent(new SessionEventArgs(session));
                }
                finally
                {
                    SessionContext.Session = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error("ServiceHost Connected error:{0}", ex);
            }
        }
        void SocketListen_DataReceived(object sender, SocketEventArgs e)
        {
            if (_kvListener.TryGetValue(e.Socket.Guid, out IClientSessionListener listener))
            {
                var session = listener.Session;
                session.Refresh();
                SessionContext.Session = session;

                try
                {
                    listener.Received(e.Data);
                }
                catch (Exception ex)
                {
                    logger.Error("ServiceHost DataReceived error:{0}", ex);
                }
                finally
                {
                    SessionContext.Session = null;
                }
            }
        }
        void SocketListen_Disconnected(object sender, SocketEventArgs e)
        {
            if (_kvListener.TryRemove(e.Socket.Guid, out IClientSessionListener listener))
            {
                var session = listener.Session;
                SessionContext.Session = session;

                try
                {
                    bool gentler = e.SocketError == SocketError.Success;
                    listener.Disconnected(gentler);
                    RaiseDisconnectedEvent(new SessionEventArgs(session));
                }
                catch (Exception ex)
                {
                    logger.Error("ServiceHost Disconnected error:{0}", ex);
                }
                finally
                {
                    SessionContext.Session = null;
                }
            }
        }
        void ClearSession(object obj)
        {
            foreach (var listener in _kvListener.Values)
            {
                var session = listener.Session;
                if (DateTime.Now - session.LastActivityTime > _setting.SessionTimeout)
                {
                    session.Close();
                    logger.Debug("Session: EndPoint:{0} HashCode:{1} timeout...", session.RemoteEndPoint, session.HashCode);
                    continue;
                }
                if (DateTime.Now - session.LastActivityTime > _setting.HeartbeatTimeout)
                {
                    session.Heartbeat();
                }
            }
        }

        private void DoClosed()
        {
            _timer.Dispose();
            _socketListen.Dispose();
            _socketListen = null;

            foreach (var listener in _kvListener.Values)
            {
                listener.Session.Close();
            }

            _kvListener.Clear();
            m_Active = false;
        }
        private void RaiseNewerSessionEvent(SessionEventArgs e)
        {
            var newerSession = NewerSession;
            if (newerSession != null)
            {
                try
                {
                    newerSession(this, e);
                }
                catch (Exception ex)
                {
                    logger.Error("ServiceHost Connected event error:" + ex);
                }
            }
        }
        private void RaiseDisconnectedEvent(SessionEventArgs e)
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
                    logger.Error("ServiceHost Disconnected event error:" + ex);
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
                    if (m_Active)
                        DoClosed();

                    _setting = null;
                    _cmdFactory = null;
                    NewerSession = null;
                    Disconnected = null;
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }
    }
}