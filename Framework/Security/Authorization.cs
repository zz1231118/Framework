using System;
using Framework.JavaScript;

namespace Framework.Security
{
    public sealed class Authorization : IAuthorization
    {
        internal Authorization()
        { }

        public Authorization(string account, long timestamp, string token)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            Account = account;
            Timestamp = timestamp;
            Token = token;
        }

        [JsonMember]
        public string Account { get; private set; }

        [JsonMember]
        public long Timestamp { get; private set; }

        [JsonMember]
        public string Token { get; private set; }
    }
}
