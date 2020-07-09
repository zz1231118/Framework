namespace Framework.Net.Remoting.App
{
    internal interface IClientSession : ISession
    {
        bool IsConnected { get; }
        IHostContext Context { get; }

        void Refresh();
        void Send(byte[] data, int offset, int count);
        void Close();
    }
}