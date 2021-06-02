namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public enum SqlExpressionType
    {
        /// <inheritdoc />
        And,
        /// <inheritdoc />
        Or,
        /// <inheritdoc />
        In,
        /// <inheritdoc />
        Is,
        /// <inheritdoc />
        As,

        /// <inheritdoc />
        GreaterThan,
        /// <inheritdoc />
        GreaterThanOrEqual,
        /// <inheritdoc />
        Equal,
        /// <inheritdoc />
        LessThanOrEqual,
        /// <inheritdoc />
        LessThan,
        /// <inheritdoc />
        NotEqual,

        /// <inheritdoc />
        Negate,
        /// <inheritdoc />
        Not,

        /// <inheritdoc />
        Symbol,
        /// <inheritdoc />
        Constant,
        /// <inheritdoc />
        Parameter,
        /// <inheritdoc />
        Array,
        /// <inheritdoc />
        MemberAccess,
        /// <inheritdoc />
        MethodCall,
        /// <inheritdoc />
        Function,

        /// <inheritdoc />
        Select,
        /// <inheritdoc />
        Insert,
        /// <inheritdoc />
        Update,
        /// <inheritdoc />
        Delete,
    }
}
