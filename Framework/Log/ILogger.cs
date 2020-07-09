using System;

namespace Framework.Log
{
    /// <summary>
    /// ILogger 接口
    /// </summary>
    public interface ILogger : IDisposable
    {
        /// <summary>
        /// 获取 Log 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 是否可用
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        bool IsEnabled(LogLevel level);

        /// <summary>
        /// 刷新缓存
        /// </summary>
        void Flush();

        /// <summary>
        /// 写出 Log
        /// </summary>
        /// <param name="level">消息等级</param>
        /// <param name="format">符合格式字符串</param>
        /// <param name="args">一个对象数组，其中包含零个或多个要设置格式的对象。</param>
        void Log(LogLevel level, string format, params object[] args);
    }
}