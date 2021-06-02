namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public class SqlParameterExpression : SqlExpression
    {
        private readonly string _name;

        internal SqlParameterExpression(string name)
        {
            _name = name;
        }

        /// <inheritdoc />
        public sealed override SqlExpressionType NodeType => SqlExpressionType.Parameter;

        /// <inheritdoc />
        public string Name => _name;

        /// <inheritdoc />
        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Parameter(this);
        }
    }
}
