using System;

namespace Framework.Net.Remoting
{
    public class ServiceSetting
    {
        public static readonly TimeSpan DefaultSessionCheckInterval = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan DefaultSessionTimeout = TimeSpan.FromHours(2);
        public static readonly TimeSpan DefaultHeartbeatTimeout = TimeSpan.FromMinutes(30);

        public ServiceSetting(ServiceEndpoint endpoint, TimeSpan sessionCheckInterval, TimeSpan sessionTimeout, TimeSpan heartbeatTimeout)
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            Endpoint = endpoint;
            SessionCheckInterval = sessionCheckInterval;
            SessionTimeout = sessionTimeout;
            HeartbeatTimeout = heartbeatTimeout;
        }
        public ServiceSetting(ServiceEndpoint endpoint)
            : this(endpoint, DefaultSessionCheckInterval, DefaultSessionTimeout, DefaultHeartbeatTimeout)
        { }

        public ServiceEndpoint Endpoint { get; private set; }
        public TimeSpan SessionCheckInterval { get; private set; }
        public TimeSpan SessionTimeout { get; private set; }
        public TimeSpan HeartbeatTimeout { get; private set; }
    }
}
