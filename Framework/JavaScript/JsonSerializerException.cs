using System;
using System.Runtime.Serialization;

namespace Framework.JavaScript
{
    /// <summary>
    /// Json 序列化异常
    /// </summary>
    [Serializable]
    public class JsonSerializerException : Exception
    {
        /// <summary>
        /// Json 序列化异常 构造函数
        /// </summary>
        public JsonSerializerException()
            : base()
        { }

        /// <summary>
        /// Json 序列化异常 构造函数
        /// </summary>
        public JsonSerializerException(string message)
            : base(message)
        { }

        /// <summary>
        /// Json 序列化异常 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public JsonSerializerException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Json 异常 构造函数
        /// </summary>
        protected JsonSerializerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}