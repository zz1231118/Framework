namespace Framework.Data
{
    /// <summary>
    /// 数据提供者类型
    /// </summary>
    public enum DbConnectionCategory : byte
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Ms Sql Server
        /// </summary>
        MsSql,
        /// <summary>
        /// My Sql Server
        /// </summary>
        MySql
    }
}