using System;
using Framework.Net.Remoting.Handler;
using Framework.Net.Remoting.Packets;

namespace Framework.Net.Remoting.App.Impl
{
    class ClientSessionListenerImpl : IClientSessionListener
    {
        private readonly IClientSession _session;
        private IPacketHandler _packetHandler;
        private bool _verifyPass;

        public ClientSessionListenerImpl(IClientSession session)
        {
            _session = session;

            Initialize();
        }

        public IClientSession Session
        {
            get { return _session; }
        }
        protected IHostContext Context
        {
            get { return Session.Context; }
        }

        public void Received(byte[] data)
        {
            if (!CheckVerify())
            {
                _session.Send(PacketFactory.CreateError("not verification"));
                _session.Close();
                return;
            }
            var package = PacketFactory.Create(data);
            _packetHandler.HandlePacket(package);
        }
        public void Disconnected(bool gentler)
        {
            (Session.Command as IDisposable)?.Dispose();
        }
        public void VerifyPass()
        {
            _verifyPass = true;
            _packetHandler = new DefaultPacketHandler(this);
        }

        private void Initialize()
        {
            _packetHandler = Context.Credentials != null
                ? (IPacketHandler)new AuthPacketHandler(this)
                : (IPacketHandler)new DefaultPacketHandler(this);
        }
        private bool CheckVerify()
        {
            if (Context.Credentials == null)
                return true;
            if (!_packetHandler.Verify)
                return true;

            return _verifyPass;
        }
    }
}