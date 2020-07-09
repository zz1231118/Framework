namespace Framework.Net.Remoting.App
{
    internal interface IClientSessionListener
    {
        IClientSession Session { get; }

        void Received(byte[] data);
        void Disconnected(bool gentler);
        void VerifyPass();
    }
}
