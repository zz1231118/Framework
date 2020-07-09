using System;
using System.Text;
using System.Threading;
using Framework.JavaScript;

namespace Framework.Net.Remoting
{
    public class Request : IDisposable
    {
        private static long _maxId;

        private bool _isDisposed;
        private ManualResetEventSlim _manualReset;

        public Request()
        {
            _manualReset = new ManualResetEventSlim(false);
            Id = NewId();
        }
        ~Request()
        {
            Dispose(false);
        }

        [JsonMember]
        public long Id { get; protected set; }
        [JsonMember]
        public string Command { get; set; }
        [JsonMember]
        public JsonArray Args { get; set; }
        internal MethodType Method { get; set; } = MethodType.Json;
        internal Response Response { get; private set; }

        private static long NewId()
        {
            return Interlocked.Increment(ref _maxId);
        }
        internal void With(Response response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            Response = response;
            _manualReset.Set();
        }
        internal void Wait()
        {
            _manualReset.Wait();
        }
        internal bool Wait(TimeSpan timeout)
        {
            return _manualReset.Wait(timeout);
        }

        internal Response CreateResponse()
        {
            var response = new Response();
            response.Id = Id;
            return response;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Request {");
            sb.Append("Cmd:").Append(Command);
            if (Args != null)
            {
                sb.Append(", ");
                sb.Append("Param:").Append(Args.ToString());
            }
            sb.Append("}");
            return sb.ToString();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                Command = null;
                Args = null;
                Response = null;
            }
            _manualReset.Dispose();
            _manualReset = null;

            _isDisposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}