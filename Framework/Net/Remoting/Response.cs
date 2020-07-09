using System;
using Framework.JavaScript;

namespace Framework.Net.Remoting
{
    internal class Response
    {
        [JsonMember]
        public long Id { get; internal set; }
        [JsonMember]
        public ResponseStatus Status { get; internal set; }
        [JsonMember]
        public Json Result { get; internal set; }
        internal MethodType Method { get; set; } = MethodType.Json;

        public T GetResult<T>()
        {
            if (Result == null)
                throw new InvalidOperationException("result is null!");

            return JsonSerializer.Deserialize<T>(Result);
        }
    }
}