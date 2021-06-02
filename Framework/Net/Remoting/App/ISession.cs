using System;
using System.Net;

namespace Framework.Net.Remoting.App
{
    public interface ISession
    {
        Guid Guid { get; }

        EndPoint LocalEndPoint { get; }

        EndPoint RemoteEndPoint { get; }

        DateTime LastActivityTime { get; }

        object Command { get; }
    }
}