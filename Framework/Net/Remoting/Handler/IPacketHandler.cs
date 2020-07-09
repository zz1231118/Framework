using Framework.Net.Remoting.App;
using Framework.Net.Remoting.Packets;

namespace Framework.Net.Remoting.Handler
{
    interface IPacketHandler
    {
        bool Verify { get; }
        IClientSession Session { get; }

        void HandlePacket(IPacket packet);
    }
}