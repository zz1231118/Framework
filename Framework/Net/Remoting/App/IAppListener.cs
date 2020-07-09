namespace Framework.Net.Remoting.App
{
    internal interface IAppListener
    {
        void Initialize();

        IClientSessionListener LoggedIn(IClientSession session);
    }
}