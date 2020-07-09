namespace Framework.Data.Expressions
{
    public class SqlDeleteLamdbaExpression : SqlExpression
    {
        internal SqlDeleteLamdbaExpression(SqlExpression from, SqlExpression condition)
        {
            From = from;
            Condition = condition;
        }

        public override SqlExpressionType NodeType => SqlExpressionType.Delete;

        public SqlExpression From { get; }

        public SqlExpression Condition { get; }

        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Delete(this);
        }
    }
}
