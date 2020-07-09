using System.Collections.Generic;

namespace Framework.Data.Expressions
{
    public class SqlUpdateLambdaExpression : SqlExpression
    {
        internal SqlUpdateLambdaExpression(IReadOnlyCollection<SqlBinaryExpression> columns, SqlExpression from, SqlExpression condition)
        {
            Columns = columns;
            From = from;
            Condition = condition;
        }

        public override SqlExpressionType NodeType => SqlExpressionType.Update;

        public IReadOnlyCollection<SqlBinaryExpression> Columns { get; }

        public SqlExpression From { get; }

        public SqlExpression Condition { get; }

        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Update(this);
        }
    }
}
