using System;
using System.Text;

namespace Framework.JavaScript
{
    /// <summary>
    /// Json 设置
    /// </summary>
    [Serializable]
    public class Jsonetting
    {
        private const string DefaultEncodingName = "GBK";

        /// <summary>
        /// 默认编码
        /// </summary>
        public static readonly Encoding DefaultEncoding = Encoding.GetEncoding(DefaultEncodingName);

        private static Encoding _encoding;

        /// <summary>
        /// Json 编码
        /// </summary>
        public static Encoding Encoding
        {
            get { return _encoding ?? DefaultEncoding; }
            set { _encoding = value; }
        }
    }
}