namespace Framework.Data.Expressions
{
    public class SqlConstantExpression : SqlExpression
    {
        private readonly object _value;

        internal SqlConstantExpression(object value)
        {
            _value = value;
        }

        public override SqlExpressionType NodeType => SqlExpressionType.Constant;

        public object Value => _value;

        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Constant(this);
        }
    }
}
