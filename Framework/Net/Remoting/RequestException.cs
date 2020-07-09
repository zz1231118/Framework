using System;

namespace Framework.Net.Remoting
{
    [Serializable]
    public class RequestException : Exception
    {
        public RequestException(string message)
            : base(message)
        { }
        public RequestException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}