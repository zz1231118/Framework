namespace Framework.Data.Expressions
{
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

        public override SqlExpressionType NodeType => _nodeType;

        public SqlExpression Left => _left;

        public SqlExpression Right => _right;

        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Binary(this);
        }
    }
}
