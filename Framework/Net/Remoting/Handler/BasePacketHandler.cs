using System;
using Framework.Net.Remoting.App;
using Framework.Net.Remoting.Packets;

namespace Framework.Net.Remoting.Handler
{
    abstract class BasePacketHandler : IPacketHandler
    {
        public BasePacketHandler(IClientSessionListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            Listener = listener;
        }

        public abstract bool Verify { get; }
        public IClientSessionListener Listener { get; }
        public IClientSession Session => Listener.Session;
        public IHostContext Context => Session.Context;

        public abstract void HandlePacket(IPacket packet);
    }
}
