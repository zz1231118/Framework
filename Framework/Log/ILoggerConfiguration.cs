using System;

namespace Framework.Log
{
    public interface ILoggerConfiguration
    {
        /// <summary>
        /// 自动刷新
        /// </summary>
        bool IsAutoFlush { get; }

        /// <summary>
        /// 刷新间隔
        /// </summary>
        TimeSpan FlushInterval { get; }
    }
}
