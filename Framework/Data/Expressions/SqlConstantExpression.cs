namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public class SqlConstantExpression : SqlExpression
    {
        private readonly object? _value;

        internal SqlConstantExpression(object? value)
        {
            _value = value;
        }

        /// <inheritdoc />
        public override SqlExpressionType NodeType => SqlExpressionType.Constant;

        /// <inheritdoc />
        public object? Value => _value;

        /// <inheritdoc />
        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Constant(this);
        }
    }
}
