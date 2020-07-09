namespace Framework.Data
{
    public enum ColumnModel : byte
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