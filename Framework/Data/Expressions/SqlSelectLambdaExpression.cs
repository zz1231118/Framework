using System.Collections.Generic;

namespace Framework.Data.Expressions
{
    public class SqlSelectLambdaExpression : SqlExpression
    {
        internal SqlSelectLambdaExpression(int? top, IReadOnlyCollection<SqlExpression> columns, SqlExpression from, SqlExpression condition, IReadOnlyCollection<SqlExpression> groups, IReadOnlyCollection<SqlExpression> sortOrders, SqlExpression having)
        {
            Top = top;
            Columns = columns;
            From = from;
            Condition = condition;
            Groups = groups;
            SortOrders = sortOrders;
            Having = having;
        }

        public override SqlExpressionType NodeType => SqlExpressionType.Select;

        public int? Top { get; }

        public IReadOnlyCollection<SqlExpression> Columns { get; }

        public SqlExpression From { get; }

        public SqlExpression Condition { get; }

        public IReadOnlyCollection<SqlExpression> Groups { get; }

        public IReadOnlyCollection<SqlExpression> SortOrders { get; }

        public SqlExpression Having { get; }

        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Select(this);
        }
    }
}
