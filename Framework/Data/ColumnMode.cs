namespace Framework.Data
{
    /// <summary>
    /// 列模式
    /// </summary>
    public enum ColumnMode : byte
    {
        /// <summary>
        /// 只读
        /// </summary>
        ReadOnly = 1,
        /// <summary>
        /// 只写
        /// </summary>
        WriteOnly = 2,
        /// <summary>
        /// 读写
        /// </summary>
        ReadWrite = 4
    }
}