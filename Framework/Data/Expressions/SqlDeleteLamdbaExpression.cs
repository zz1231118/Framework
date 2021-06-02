namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public class SqlDeleteLamdbaExpression : SqlExpression
    {
        internal SqlDeleteLamdbaExpression(SqlExpression from, SqlExpression? condition)
        {
            From = from;
            Condition = condition;
        }

        /// <inheritdoc />
        public override SqlExpressionType NodeType => SqlExpressionType.Delete;

        /// <inheritdoc />
        public SqlExpression From { get; }

        /// <inheritdoc />
        public SqlExpression? Condition { get; }

        /// <inheritdoc />
        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Delete(this);
        }
    }
}
