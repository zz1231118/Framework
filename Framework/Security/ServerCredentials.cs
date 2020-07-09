using System;

namespace Framework.Security
{
    public class ServerCredentials : IServerCredentials
    {
        private readonly string accessKey;

        public ServerCredentials(string accessKey)
        {
            if (accessKey == null)
                throw new ArgumentNullException(nameof(accessKey));

            this.accessKey = accessKey;
        }

        public string AccessKey => accessKey;

        public virtual bool Authenticate(IAuthorization authorization)
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            var tokenText = authorization.Account + accessKey + authorization.Timestamp;
            var array = tokenText.ToCharArray();

            Array.Sort(array);
            var encryptText = new string(array);
            var token = ClientCredential.Md5(encryptText);
            return token.Equals(authorization.Token, StringComparison.InvariantCulture);
        }
    }
}
