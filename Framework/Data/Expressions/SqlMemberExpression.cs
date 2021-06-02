namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public class SqlMemberExpression : SqlExpression
    {
        private readonly string _name;

        internal SqlMemberExpression(string name)
        {
            _name = name;
        }

        /// <inheritdoc />
        public sealed override SqlExpressionType NodeType => SqlExpressionType.MemberAccess;

        /// <inheritdoc />
        public string Name => _name;

        /// <inheritdoc />
        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Member(this);
        }
    }
}
