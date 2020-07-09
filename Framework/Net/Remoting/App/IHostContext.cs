using Framework.Security;

namespace Framework.Net.Remoting.App
{
    interface IHostContext
    {
        IServerCredentials Credentials { get; }

        ServiceContractAttribute ServiceContract { get; }
    }
}
