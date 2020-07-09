namespace Framework.Data.Expressions
{
    public class SqlUnaryExpression : SqlExpression
    {
        private readonly SqlExpressionType _nodeType;
        private readonly SqlExpression _expression;

        internal SqlUnaryExpression(SqlExpressionType nodeType, SqlExpression expression)
        {
            _nodeType = nodeType;
            _expression = expression;
        }

        public override SqlExpressionType NodeType => _nodeType;

        public SqlExpression Expression => _expression;

        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Unary(this);
        }
    }
}
