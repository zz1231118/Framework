using System;
using System.Security.Cryptography;
using System.Text;

namespace Framework.Net.WebSockets
{
    class Hybi10HandshakeProcessor : Hybi00HandshakeProcessor
    {
        private const string ServerKey = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public Hybi10HandshakeProcessor(WebSocketListener webSocketListener, Encoding encoding)
            : base(webSocketListener, encoding)
        { }

        public Hybi10HandshakeProcessor(WebSocketListener webSocketListener)
            : this(webSocketListener, Encoding.UTF8)
        { }

        protected override bool CheckSignKey(HandshakeData handshakeData)
        {
            if (handshakeData.WebSocketVersion < WebSocketListener.Version)
            {
                return base.CheckSignKey(handshakeData);
            }
            if (handshakeData.ParamItems.TryGet(HandshakeHeadKeys.SecAccept, out string accecpKey) &&
                handshakeData.ParamItems.TryGet(HandshakeHeadKeys.SecSignKey, out string signKey))
            {
                return string.Equals(signKey, accecpKey);
            }
            return false;
        }

        protected override bool ResponseHandshake(WebSocket socket, HandshakeData handshakeData)
        {
            if (handshakeData.WebSocketVersion < WebSocketListener.Version)
            {
                return base.ResponseHandshake(socket, handshakeData);
            }

            string secKeyAccept = GenreateKey(handshakeData);
            StringBuilder response = new StringBuilder();
            response.AppendLine(HandshakeHeadKeys.RespHead_10);
            response.AppendLine(HandshakeHeadKeys.RespUpgrade);
            response.AppendLine(HandshakeHeadKeys.RespConnection);
            response.AppendLine(string.Format(HandshakeHeadKeys.RespAccept, secKeyAccept));

            if (!string.IsNullOrEmpty(handshakeData.Protocol))
            {
                response.AppendLine(string.Format(HandshakeHeadKeys.RespProtocol, handshakeData.Protocol));
            }
            response.AppendLine();
            socket.PostSend(response.ToString());
            return true;
        }

        private string GenreateKey(HandshakeData handshakeData)
        {
            if (handshakeData.ParamItems.TryGet(HandshakeHeadKeys.SecKey, out string key))
            {
                return GenreateKey(key);
            }
            return string.Empty;
        }

        private static string GenreateKey(string key)
        {
            try
            {
                using (var sha1 = SHA1.Create())
                {
                    byte[] encryptionString = sha1.ComputeHash(Encoding.ASCII.GetBytes(key + ServerKey));
                    return Convert.ToBase64String(encryptionString);
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
