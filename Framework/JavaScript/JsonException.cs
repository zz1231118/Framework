using System;
using System.Runtime.Serialization;

namespace Framework.JavaScript
{
    /// <summary>
    /// Json 异常
    /// </summary>
    [Serializable]
    public class JsonException : Exception
    {
        /// <summary>
        /// Json 异常 构造函数
        /// </summary>
        public JsonException()
            : base()
        { }

        /// <summary>
        /// Json 异常 构造函数
        /// </summary>
        public JsonException(string message)
            : base(message)
        { }

        /// <summary>
        /// Json 异常 构造函数
        /// </summary>
        public JsonException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Json 异常 构造函数
        /// </summary>
        protected JsonException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}