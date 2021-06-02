using System.Collections.Generic;

namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public class SqlUpdateLambdaExpression : SqlExpression
    {
        internal SqlUpdateLambdaExpression(IReadOnlyCollection<SqlBinaryExpression> columns, SqlExpression from, SqlExpression? condition)
        {
            Columns = columns;
            From = from;
            Condition = condition;
        }

        /// <inheritdoc />
        public override SqlExpressionType NodeType => SqlExpressionType.Update;

        /// <inheritdoc />
        public IReadOnlyCollection<SqlBinaryExpression> Columns { get; }

        /// <inheritdoc />
        public SqlExpression From { get; }

        /// <inheritdoc />
        public SqlExpression? Condition { get; }

        /// <inheritdoc />
        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Update(this);
        }
    }
}
