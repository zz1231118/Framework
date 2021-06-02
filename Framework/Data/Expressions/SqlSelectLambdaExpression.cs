using System.Collections.Generic;

namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public class SqlSelectLambdaExpression : SqlExpression
    {
        internal SqlSelectLambdaExpression(int? top, IReadOnlyCollection<SqlExpression> columns, SqlExpression from, SqlExpression? condition, IReadOnlyCollection<SqlExpression>? groups, IReadOnlyCollection<SqlExpression>? sortOrders, SqlExpression? having)
        {
            Top = top;
            Columns = columns;
            From = from;
            Condition = condition;
            Groups = groups;
            SortOrders = sortOrders;
            Having = having;
        }

        /// <inheritdoc />
        public override SqlExpressionType NodeType => SqlExpressionType.Select;

        /// <inheritdoc />
        public int? Top { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<SqlExpression> Columns { get; }

        /// <inheritdoc />
        public SqlExpression From { get; }

        /// <inheritdoc />
        public SqlExpression? Condition { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<SqlExpression>? Groups { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<SqlExpression>? SortOrders { get; }

        /// <inheritdoc />
        public SqlExpression? Having { get; }

        /// <inheritdoc />
        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Select(this);
        }
    }
}
