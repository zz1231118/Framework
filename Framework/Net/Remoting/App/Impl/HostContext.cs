using Framework.Security;

namespace Framework.Net.Remoting.App.Impl
{
    internal class HostContext : IHostContext
    {
        public IServerCredentials Credentials { get; set; }
        public ServiceContractAttribute ServiceContract { get; set; }
    }
}