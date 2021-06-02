using System;

namespace Framework.Log
{
    /// <summary>
    /// Logger 配置
    /// </summary>
    public class LoggerConfiguration : ILoggerConfiguration
    {
        /// <summary>
        /// 默认配置
        /// </summary>
        public static readonly ILoggerConfiguration Default = new LoggerConfiguration();

        /// <summary>
        /// 自动刷新
        /// </summary>
        public bool IsAutoFlush { get; set; } = true;

        /// <summary>
        /// 刷新间隔
        /// </summary>
        public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(5);
    }
}
