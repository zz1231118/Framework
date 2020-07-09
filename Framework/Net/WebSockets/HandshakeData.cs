using System.Collections.Generic;

namespace Framework.Net.WebSockets
{
    internal class HandshakeData
    {
        public HandshakeData()
        {
            UriSchema = "ws";
            ParamItems = new HandshakeItems();
        }

        public bool IsClient { get; set; }

        public bool Handshaked { get; set; }

        public string Url
        {
            get { return string.Format(HandshakeHeadKeys.RespUrl, UriSchema, Host, UrlPath); }
        }

        public string UriSchema { get; set; }

        public string Method { get; set; }

        public string UrlPath { get; set; }

        public string HttpVersion { get; set; }

        public string Host { get; set; }

        public int WebSocketVersion { get; set; }

        public HandshakeItems ParamItems { get; set; }

        public Dictionary<string, string> Cookies { get; set; }

        public string Protocol { get; set; }

        public string QueueString
        {
            get
            {
                int index = UrlPath.IndexOf('?');
                return index > -1 ? UrlPath.Substring(index) : UrlPath;
            }
        }
    }

    internal class HandshakeItems : Dictionary<string, object>
    {
        public bool TryGet<T>(string key, out T value)
        {
            value = default(T);
            if (TryGetValue(key, out object obj))
            {
                value = (T)obj;
                return true;
            }
            return false;
        }
    }
}
