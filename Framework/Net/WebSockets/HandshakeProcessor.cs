using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Framework.Log;

namespace Framework.Net.WebSockets
{
    internal abstract class HandshakeProcessor
    {
        private readonly ILogger logger = Logger.GetLogger<HandshakeProcessor>();
        private static readonly byte[] HandshakeEndBytes = Encoding.UTF8.GetBytes("\r\n\r\n");
        private readonly WebSocketListener webSocketListener;

        public HandshakeProcessor(WebSocketListener webSocketListener, Encoding encoding)
        {
            this.webSocketListener = webSocketListener;
            Encoding = encoding;
        }

        public WebSocketListener WebSocketListener => webSocketListener;

        public Encoding Encoding { get; }

        protected abstract bool ResponseHandshake(WebSocket socket, HandshakeData handshakeData);

        protected abstract bool CheckSignKey(HandshakeData handshakeData);

        protected virtual bool TryParseHandshake(string message, HandshakeData handshakeData, out string error)
        {
            using (var reader = new StringReader(message))
            {
                error = string.Empty;
                string headData = reader.ReadLine() ?? "";
                var headParams = headData.Split(' ');
                if (headParams.Length < 3 ||
                    (headParams[0] != HandshakeHeadKeys.Method && headParams[0] != HandshakeHeadKeys.HttpVersion))
                {
                    return false;
                }
                if (headParams[0] != HandshakeHeadKeys.HttpVersion)
                {
                    if (webSocketListener.IsSecurity)
                    {
                        handshakeData.UriSchema = "wss";
                    }
                    handshakeData.Method = headParams[0];
                    handshakeData.UrlPath = headParams[1];
                    handshakeData.HttpVersion = headParams[2];
                }

                string paramStr;
                while (!string.IsNullOrEmpty(paramStr = reader.ReadLine()))
                {
                    //ex: Upgrade: WebSocket
                    var paramArr = paramStr.Split(':');
                    if (paramArr.Length < 2)
                    {
                        continue;
                    }
                    string key = paramArr[0].Trim();
                    //value includ spance char
                    string value = string.Join("", paramArr, 1, paramArr.Length - 1);
                    if (value.Length > 1) value = value.Substring(1); //pre char is spance
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }
                    if (string.Equals(HandshakeHeadKeys.Host, key))
                    {
                        handshakeData.Host = value;
                        continue;
                    }
                    if (string.Equals(HandshakeHeadKeys.SecVersion, key))
                    {
                        handshakeData.WebSocketVersion = int.Parse(value);
                        continue;
                    }
                    if (string.Equals(HandshakeHeadKeys.SecProtocol, key))
                    {
                        handshakeData.Protocol = GetFirstProtocol(value);
                        continue;
                    }
                    if (string.Equals(HandshakeHeadKeys.Cookie, key))
                    {
                        ParseCookies(handshakeData, value);
                        continue;
                    }
                    handshakeData.ParamItems[key] = value;
                }

                if (handshakeData.ParamItems.ContainsKey(HandshakeHeadKeys.Upgrade) &&
                    handshakeData.ParamItems.ContainsKey(HandshakeHeadKeys.Connection))
                {
                    return true;
                }
                error = "not support websocket ";
            }

            return false;
        }

        protected virtual string GetFirstProtocol(string protocol)
        {
            if (string.IsNullOrEmpty(protocol))
            {
                return string.Empty;
            }
            var arrNames = protocol.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return arrNames.Length > 0 ? arrNames[0] : string.Empty;
        }

        protected void ParseCookies(HandshakeData handshake, string cookieStr)
        {
            if (handshake == null) return;
            if (handshake.Cookies == null)
            {
                handshake.Cookies = new Dictionary<string, string>();
            }
            var array = cookieStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in array)
            {
                var kvs = item.Split('=');
                if (kvs.Length == 2 && !string.IsNullOrEmpty(kvs[0]))
                {
                    handshake.Cookies[Uri.UnescapeDataString(kvs[0])] = Uri.UnescapeDataString(kvs[1].Trim());
                }
            }
        }

        internal HandshakeResult Receive(SocketAsyncEventArgs ioEventArgs, WSDataToken dataToken, byte[] data)
        {
            if (dataToken.byteArrayForHandshake == null)
            {
                dataToken.byteArrayForHandshake = new List<byte>(data);
            }
            else
            {
                dataToken.byteArrayForHandshake.AddRange(data);
            }
            var socket = (WebSocket)dataToken.Socket;
            var buffer = dataToken.byteArrayForHandshake.ToArray();
            int headLength = MathUtils.IndexOf(buffer, HandshakeEndBytes);
            if (headLength < 0)
            {
                //data not complate, wait receive
                return HandshakeResult.Wait;
            }
            headLength += HandshakeEndBytes.Length;
            byte[] headBytes = new byte[headLength];
            Buffer.BlockCopy(buffer, 0, headBytes, 0, headBytes.Length);

            string message = Encoding.GetString(headBytes);
            HandshakeData handshakeData = socket.Handshake;
            if (TryParseHandshake(message, handshakeData, out string error))
            {
                if (handshakeData.ParamItems.ContainsKey(HandshakeHeadKeys.SecKey1) &&
                    handshakeData.ParamItems.ContainsKey(HandshakeHeadKeys.SecKey2))
                {
                    int remainBytesNum = buffer.Length - headLength;
                    if (!handshakeData.IsClient && remainBytesNum == 8)
                    {
                        byte[] secKey3Bytes = new byte[remainBytesNum];
                        Buffer.BlockCopy(buffer, headBytes.Length, secKey3Bytes, 0, secKey3Bytes.Length);
                        handshakeData.ParamItems[HandshakeHeadKeys.SecKey3] = secKey3Bytes;
                    }
                    else if (handshakeData.IsClient && remainBytesNum == 16)
                    {
                        byte[] secKey3Bytes = new byte[remainBytesNum];
                        Buffer.BlockCopy(buffer, headBytes.Length, secKey3Bytes, 0, secKey3Bytes.Length);
                        handshakeData.ParamItems[HandshakeHeadKeys.SecAccept] = secKey3Bytes;
                    }
                    else
                    {
                        //data not complate, wait receive
                        return HandshakeResult.Wait;
                    }
                }
                if (!handshakeData.IsClient)
                {
                    if (!ResponseHandshake(socket, handshakeData))
                    {
                        //TraceLog.ReleaseWriteDebug("Client {0} handshake fail, message:\r\n{2}", session.Socket.RemoteEndPoint, message);
                        return HandshakeResult.Close;
                    }
                    dataToken.byteArrayForHandshake = null;
                    socket.Handshake.Handshaked = true;
                    return HandshakeResult.Success;
                }
                if (CheckSignKey(handshakeData))
                {
                    dataToken.byteArrayForHandshake = null;
                    socket.Handshake.Handshaked = true;
                    return HandshakeResult.Success;
                }
                return HandshakeResult.Close;
            }

            logger.Warn("Client {0} handshake {1}error, detail\r\n{2}", socket.RemoteEndPoint, error, message);
            return HandshakeResult.Close;
        }
    }
}
