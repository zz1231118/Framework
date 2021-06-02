namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public class SqlUnaryExpression : SqlExpression
    {
        private readonly SqlExpressionType _nodeType;
        private readonly SqlExpression _expression;

        internal SqlUnaryExpression(SqlExpressionType nodeType, SqlExpression expression)
        {
            _nodeType = nodeType;
            _expression = expression;
        }

        /// <inheritdoc />
        public override SqlExpressionType NodeType => _nodeType;

        /// <inheritdoc />
        public SqlExpression Expression => _expression;

        /// <inheritdoc />
        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Unary(this);
        }
    }
}
