namespace Framework.Net.WebSockets
{
    class Rfc6455CloseStatusCode : CloseStatusCode
    {
        public Rfc6455CloseStatusCode()
        {
            InternalServerError = 1010;
            TLSHandshake = 1015;
        }

        /// <summary>
        /// 1011 RFC-6455
        /// </summary>
        public int InternalServerError { get; protected set; }

        /// <summary>
        /// 1015 RFC-6455
        /// </summary>
        public int TLSHandshake { get; protected set; }
    }
}
