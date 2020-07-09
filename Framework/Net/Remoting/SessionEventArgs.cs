using System;
using Framework.Net.Remoting.App;

namespace Framework.Net.Remoting
{
    public class SessionEventArgs : EventArgs
    {
        public SessionEventArgs(ISession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            Session = session;
        }

        public ISession Session { get; private set; }
    }
}
