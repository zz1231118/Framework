using System;
using System.Linq;
using System.Text;
using Framework.JavaScript;

namespace Framework.Net.Remoting.Packets
{
    class Packet : IPacket
    {
        private readonly static Encoding _encoding = Encoding.UTF8;

        public Packet(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException(nameof(data));
            if (data[0] != Version)
                throw new ArgumentException("invalid data, version error!");

            Data = data;
            Action = (ActionType)Enum.ToObject(typeof(ActionType), data[1]);
            if (data.Length > 2)
            {
                if (data.Length < 3)
                    throw new ArgumentException("data error!");

                Method = (MethodType)Enum.ToObject(typeof(MethodType), data[2]);
                switch (Method)
                {
                    case MethodType.Json:
                        var str = _encoding.GetString(data, 3, data.Length - 3);
                        Json = Json.Parse(str);
                        break;
                    case MethodType.Bson:
                        Json = Json.Parse(data.Skip(3).ToArray());
                        break;
                    default:
                        throw new ArgumentException("unknown MethodType: " + Method.ToString());
                }
            }
        }
        public Packet(ActionType actionType, MethodType method = MethodType.Json, Json json = null)
        {
            Action = actionType;
            if (json == null)
            {
                Data = new byte[2] { Version, (byte)actionType };
                return;
            }

            Method = method;
            Json = json;

            switch (Method)
            {
                case MethodType.Json:
                    var str = json.ToString();
                    var byteAryForStr = _encoding.GetBytes(str);
                    Data = new byte[byteAryForStr.Length + 3];
                    Data[0] = Version;
                    Data[1] = (byte)actionType;
                    Data[2] = (byte)method;
                    Array.Copy(byteAryForStr, 0, Data, 3, byteAryForStr.Length);
                    break;
                case MethodType.Bson:
                    var byteAryForBson = json.ToBinary();
                    Data = new byte[byteAryForBson.Length + 3];
                    Data[0] = Version;
                    Data[1] = (byte)actionType;
                    Data[2] = (byte)method;
                    Array.Copy(byteAryForBson, 0, Data, 3, byteAryForBson.Length);
                    break;
                default:
                    throw new ArgumentException("unknown MethodType: " + Method.ToString());
            }
        }

        public byte Version => 2;
        public byte[] Data
        {
            get;
            private set;
        }
        public ActionType Action
        {
            get;
            private set;
        }
        public MethodType Method
        {
            get;
            private set;
        }
        public Json Json
        {
            get;
            private set;
        }

        public T GetValue<T>()
        {
            return JsonSerializer.Deserialize<T>(Json);
        }
    }
}