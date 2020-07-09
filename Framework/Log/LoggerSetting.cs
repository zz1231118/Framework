using System;
using System.IO;
using System.Text;

namespace Framework.Log
{
    public class LoggerSetting
    {
        public const int DefaultMaximumFileLength = 1024 * 1024 * 10;
        public const int DefaultMaximumFileCacheMessageCount = 20000;
        public const int DefaultMaximumCacheMessageCount = 200;
        public const bool DefaultAutoFlush = true;

        public static readonly string DefaultBaseDirectory;
        public static readonly TimeSpan DefaultFlushInterval = TimeSpan.FromSeconds(5);
        public static readonly Encoding DefaultEncoding = Encoding.UTF8;
        public static readonly LoggerSetting Default;

        static LoggerSetting()
        {
            var appDomain = AppDomain.CurrentDomain;
            DefaultBaseDirectory = Path.Combine(appDomain.BaseDirectory, "Log");
            Default = new LoggerSetting(DefaultBaseDirectory);
        }

        public LoggerSetting(string baseDirectory)
            : this(baseDirectory, DefaultMaximumFileLength, DefaultMaximumFileCacheMessageCount, DefaultMaximumCacheMessageCount, DefaultAutoFlush, DefaultFlushInterval, DefaultEncoding)
        { }

        public LoggerSetting(string baseDirectory, bool isAutoFlush, TimeSpan flushInterval)
            : this(baseDirectory, DefaultMaximumFileLength, DefaultMaximumFileCacheMessageCount, DefaultMaximumCacheMessageCount, isAutoFlush, flushInterval, DefaultEncoding)
        { }

        public LoggerSetting(string baseDirectory, int maximumFileLength, int maximumFileCacheMessageCount, int maximumCacheMessageCount, bool isAutoFlush, TimeSpan flushInterval, Encoding encoding)
        {
            if (baseDirectory == null)
                throw new ArgumentNullException(nameof(baseDirectory));
            if (encoding == null)
                throw new ArgumentException(nameof(encoding));

            BaseDirectory = baseDirectory;
            MaximumFileLength = maximumFileLength;
            MaximumFileCacheMessageCount = maximumFileCacheMessageCount;
            MaximumCacheMessageCount = maximumCacheMessageCount;
            IsAutoFlush = isAutoFlush;
            FlushInterval = flushInterval;
            Encoding = encoding;
        }

        /// <summary>
        /// 编码
        /// </summary>
        public Encoding Encoding { get; private set; }

        /// <summary>
        /// 存放Log 的文件夹全名
        /// </summary>
        public string BaseDirectory { get; private set; }

        /// <summary>
        /// 最大文件大小
        /// </summary>
        public int MaximumFileLength { get; private set; }

        /// <summary>
        /// 最大缓存数
        /// </summary>
        public int MaximumCacheMessageCount { get; private set; }

        /// <summary>
        /// 最文件缓存大缓存数
        /// </summary>
        public int MaximumFileCacheMessageCount { get; private set; }

        /// <summary>
        /// 自动刷新
        /// </summary>
        public bool IsAutoFlush { get; private set; }

        /// <summary>
        /// 刷新间隔
        /// </summary>
        public TimeSpan FlushInterval { get; private set; }
    }
}