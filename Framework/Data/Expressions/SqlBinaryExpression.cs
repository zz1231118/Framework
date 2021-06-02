namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public class SqlBinaryExpression : SqlExpression
    {
        private readonly SqlExpressionType _nodeType;
        private readonly SqlExpression _left;
        private readonly SqlExpression _right;

        internal SqlBinaryExpression(SqlExpressionType nodeType, SqlExpression left, SqlExpression right)
        {
            _nodeType = nodeType;
            _left = left;
            _right = right;
        }

        /// <inheritdoc />
        public override SqlExpressionType NodeType => _nodeType;

        /// <inheritdoc />
        public SqlExpression Left => _left;

        /// <inheritdoc />
        public SqlExpression Right => _right;

        /// <inheritdoc />
        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Binary(this);
        }
    }
}
