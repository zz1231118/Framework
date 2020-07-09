using System;
using System.Runtime.Serialization;

namespace Framework.JavaScript
{
    /// <summary>
    /// Json 序列化异常
    /// </summary>
    [Serializable]
    public class JsonerializerException : Exception
    {
        /// <summary>
        /// Json 序列化异常 构造函数
        /// </summary>
        public JsonerializerException()
            : base()
        { }

        /// <summary>
        /// Json 序列化异常 构造函数
        /// </summary>
        public JsonerializerException(string message)
            : base(message)
        { }

        /// <summary>
        /// Json 序列化异常 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public JsonerializerException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Json 异常 构造函数
        /// </summary>
        protected JsonerializerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}