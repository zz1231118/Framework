using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using Framework.JavaScript;
using Framework.Log;
using Framework.Net.Remoting.App;
using Framework.Net.Remoting.Packets;
using Framework.Reflection;

namespace Framework.Net.Remoting.Handler
{
    class DefaultPacketHandler : BasePacketHandler
    {
        private static readonly ConcurrentDictionary<MethodBase, OperationContractAttribute> _kvOperContract = new ConcurrentDictionary<MethodBase, OperationContractAttribute>();
        private static readonly Func<MethodBase, OperationContractAttribute> _operContractFactory = key => key.GetCustomAttribute<OperationContractAttribute>(false);
        private static readonly ILogger logger = Logger.GetLogger<DefaultPacketHandler>();

        public DefaultPacketHandler(IClientSessionListener listener)
            : base(listener)
        { }

        public sealed override bool Verify => true;
        public object Command => Session.Command;

        public override void HandlePacket(IPacket packet)
        {
            switch (packet.Version)
            {
                case 2:
                    PackageHandler(packet as Packet);
                    break;
                default:
                    Session.Send(PacketFactory.CreateError("Packet Unknown Version:" + packet.Version.ToString()));
                    Session.Close();
                    break;
            }
        }
        private void PackageHandler(Packet package)
        {
            switch (package.Action)
            {
                case ActionType.Heartbeat:
                    Session.Refresh();
                    break;
                case ActionType.Request:
                    SetupRequest(package.GetValue<Request>());
                    break;
                case ActionType.Response:
                    break;
                default:
                    var errPack = PacketFactory.CreateError("Unknown ActionType:{0}", package.Action);
                    Session.Send(errPack);
                    Session.Close();
                    break;
            }
        }

        private void SetupRequest(Request request)
        {
            switch (Context.ServiceContract.ConcurrencyMode)
            {
                case ConcurrencyMode.Single:
                case ConcurrencyMode.Reentrant:
                    {
                        var response = Response(request);
                        var respPack = PacketFactory.Create(response);
                        Session.Send(respPack);
                    }
                    break;
                case ConcurrencyMode.Multiple:
                    ThreadPool.QueueUserWorkItem(obj =>
                    {
                        var response = Response(request);
                        var packet = PacketFactory.Create(response);
                        Session.Send(packet);
                    }, null);
                    break;
            }
        }
        private Response Response(Request request)
        {
            var response = request.CreateResponse();
            var type = Command.GetType();
            if (!ILMethodFactory.TryGetOrCreate(type, request.Command, out FastInvokeMethod[] fastMethods))
            {
                response.Status = ResponseStatus.Error;
                response.Result = "未找到指定方法:" + request.Command;
                return response;
            }
            var paramCount = request.Args?.Count ?? 0;
            FastInvokeMethod fastMethod = fastMethods.FirstOrDefault(p => p.GetParameters().Length == paramCount);
            if (fastMethod == null)
            {
                response.Status = ResponseStatus.Error;
                response.Result = "未找到指定方法:" + request.Command;
                return response;
            }
            var operationContract = _kvOperContract.GetOrAdd(fastMethod.MethodInfo, _operContractFactory);
            if (operationContract == null)
            {
                response.Status = ResponseStatus.Error;
                response.Result = "不是协定方法:" + request.Command;
                return response;
            }
            try
            {
                var parameters = fastMethod.GetParameters();
                var objParams = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    var json = request.Args[i];
                    objParams[i] = JsonSerializer.Deserialize(json, parameter.ParameterType);
                }

                var handler = fastMethod.Handler;
                var result = handler.Invoke(Command, objParams);
                if (fastMethod.ReturnType != typeof(void))
                    response.Result = JsonSerializer.Serialize(result);

                response.Status = ResponseStatus.Success;
                response.Method = operationContract.Method;
            }
            catch (Exception ex)
            {
                response.Status = ResponseStatus.Error;
                response.Result = ex.Message;
                logger.Warn("Request: {0} error: {1}", request, ex);
            }
            return response;
        }
    }
}
