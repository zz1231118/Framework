namespace Framework.Data.Expressions
{
    public sealed class SqlSymbolExpression : SqlExpression
    {
        private readonly string token;

        internal SqlSymbolExpression(string token)
        {
            this.token = token;
        }

        public string Token => token;

        public sealed override SqlExpressionType NodeType => SqlExpressionType.Symbol;

        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Symbol(this);
        }
    }
}
