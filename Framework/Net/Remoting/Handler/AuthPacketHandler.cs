using Framework.Net.Remoting.App;
using Framework.Net.Remoting.Packets;
using Framework.Security;

namespace Framework.Net.Remoting.Handler
{
    class AuthPacketHandler : BasePacketHandler
    {
        public AuthPacketHandler(IClientSessionListener listener)
            : base(listener)
        { }

        public sealed override bool Verify => false;

        public override void HandlePacket(IPacket packet)
        {
            if (packet is Packet)
            {
                var package = packet as Packet;
                if (package.Action != ActionType.Validate)
                {
                    NotVerified();
                    return;
                }
            }
            else
            {
                NotVerified();
                return;
            }

            var authentication = packet.GetValue<Authorization>();
            Validation(authentication);
        }
        private void Validation(Authorization authentication)
        {
            if (!Context.Credentials.Authenticate(authentication))
            {
                ValidationFailure();
                return;
            }

            Listener.VerifyPass();
        }
        private void NotVerified()
        {
            Session.Send(PacketFactory.CreateError("not verified"));
            Session.Close();
        }
        private void ValidationFailure()
        {
            Session.Send(PacketFactory.CreateError("verification fail"));
            Session.Close();
        }
    }
}
