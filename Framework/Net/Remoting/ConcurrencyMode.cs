namespace Framework.Net.Remoting
{
    public enum ConcurrencyMode : byte
    {
        /// <summary>
        /// 服务实例是单线程的，且不接受可重入调用。
        /// </summary>
        Single = 0,
        /// <summary>
        /// 服务实例是单线程的，且接受可重入调用。
        /// </summary>
        Reentrant = 1,
        /// <summary>
        /// 服务实例是多线程的。无同步保证。
        /// </summary>
        Multiple = 2,
    }
}
