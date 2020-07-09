using System;
using Framework.JavaScript;

namespace Framework.Net.Remoting.Packets
{
    class PacketFactory
    {
        public static IPacket Create(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("data error!");

            switch (data[0])
            {
                case 2:
                    return new Packet(data);
                default:
                    throw new InvalidOperationException(string.Format("unknown package version: {0}", data[0].ToString()));
            }
        }
        public static IPacket Create(Response respose)
        {
            var json = JsonSerializer.Serialize(respose);
            return new Packet(ActionType.Response, respose.Method, json);
        }
        public static IPacket CreateError(string format, params object[] args)
        {
            var json = new JsonValue(string.Format(format, args));
            return new Packet(ActionType.Error, MethodType.Json, json);
        }
    }
}