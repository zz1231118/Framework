namespace Framework.Data
{
    public enum DbCommandMode : byte
    {
        /// <summary>
        /// Select
        /// </summary>
        Select = 0,
        /// <summary>
        /// Update
        /// </summary>
        Update,
        /// <summary>
        /// Insert
        /// </summary>
        Insert,
        /// <summary>
        /// Delete
        /// </summary>
        Delete,
    }
}
