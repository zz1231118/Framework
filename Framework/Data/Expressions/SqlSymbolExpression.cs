namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public sealed class SqlSymbolExpression : SqlExpression
    {
        private readonly string token;

        internal SqlSymbolExpression(string token)
        {
            this.token = token;
        }

        /// <inheritdoc />
        public string Token => token;

        /// <inheritdoc />
        public sealed override SqlExpressionType NodeType => SqlExpressionType.Symbol;

        /// <inheritdoc />
        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Symbol(this);
        }
    }
}
