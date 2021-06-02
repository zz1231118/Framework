namespace Framework.Log
{
    /// <summary>
    /// ILogger 接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 获取 Log 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 写出 Log
        /// </summary>
        /// <param name="level">消息等级</param>
        /// <param name="format">对象</param>
        /// <param name="args">一个对象数组，其中包含零个或多个要设置格式的对象。</param>
        void Log(Level level, string format, params object[] args);
    }
}