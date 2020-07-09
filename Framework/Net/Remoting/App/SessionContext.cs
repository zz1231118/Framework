using System;

namespace Framework.Net.Remoting.App
{
    public static class SessionContext
    {
        [ThreadStatic]
        private static ISession _localSession;

        public static ISession Session
        {
            get { return _localSession; }
            internal set { _localSession = value; }
        }
    }
}
