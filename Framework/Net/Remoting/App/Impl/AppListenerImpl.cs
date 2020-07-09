namespace Framework.Net.Remoting.App.Impl
{
    class AppListenerImpl : IAppListener
    {
        public void Initialize()
        { }
        public IClientSessionListener LoggedIn(IClientSession session)
        {
            return new ClientSessionListenerImpl(session);
        }
    }
}