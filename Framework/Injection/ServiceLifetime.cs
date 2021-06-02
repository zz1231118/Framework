namespace Framework.Injection
{
    /// <summary>
    /// 服务生命周期
    /// </summary>
    public enum ServiceLifetime : byte
    {
        /// <summary>
        /// 唯一的实例。
        /// </summary>
        Singleton,
        /// <summary>
        /// 临时的实例。
        /// </summary>
        Transient,
    }
}
