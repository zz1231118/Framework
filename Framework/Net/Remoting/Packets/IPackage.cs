using System;
using Framework.Net.Remoting.App;

namespace Framework.Net.Remoting.Packets
{
    public interface IPacket
    {
        byte Version { get; }
        byte[] Data { get; }

        T GetValue<T>();
    }
    static class SessionExtension
    {
        public static void Send(this IClientSession session, IPacket packet)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (packet == null)
                throw new ArgumentNullException(nameof(packet));

            var byteAryForMsg = packet.Data;
            session.Send(byteAryForMsg, 0, byteAryForMsg.Length);
        }
        public static void Heartbeat(this IClientSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            Send(session, new Packet(ActionType.Heartbeat, MethodType.Json, null));
        }
    }
}