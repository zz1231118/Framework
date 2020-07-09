using System;
using System.Runtime.Serialization;

namespace Framework.JavaScript
{
    /// <summary>
    /// DataFromat 异常
    /// </summary>
    [Serializable]
    public class JsonFormatException : Exception
    {
        /// <summary>
        /// DataFromat 异常 构造函数
        /// </summary>
        public JsonFormatException()
            : base()
        { }

        /// <summary>
        /// DataFromat 异常 构造函数
        /// </summary>
        public JsonFormatException(string message)
            : base(message)
        { }

        /// <summary>
        /// DataFromat 异常 构造函数
        /// </summary>
        public JsonFormatException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Json 异常 构造函数
        /// </summary>
        protected JsonFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}