namespace Framework.Configuration.Hocon
{
    /// <inheritdoc />
    public enum TokenKind : byte
    {
        /// <inheritdoc />
        Comment,
        /// <inheritdoc />
        Key,
        /// <inheritdoc />
        LiteralValue,
        /// <inheritdoc />
        Assign,
        /// <inheritdoc />
        ObjectStart,
        /// <inheritdoc />
        ObjectEnd,
        /// <inheritdoc />
        Dot,
        /// <inheritdoc />
        EoF,
        /// <inheritdoc />
        ArrayStart,
        /// <inheritdoc />
        ArrayEnd,
        /// <inheritdoc />
        Comma,
        /// <inheritdoc />
        Substitute,
        /// <inheritdoc />
        Include,
    }
}
