namespace Framework.Data
{
    /// <summary>
    /// 索引类别
    /// </summary>
    public enum IndexCategory : byte
    {
        /// <summary>
        /// 聚集索引
        /// </summary>
        Clustered,
        /// <summary>
        /// 非聚集索引
        /// </summary>
        NonClustered,
    }
}
