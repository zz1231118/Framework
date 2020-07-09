using System.Collections.Generic;

namespace Framework.Data.Expressions
{
    public class SqlInsertLambdaExpression : SqlExpression
    {
        internal SqlInsertLambdaExpression(IReadOnlyCollection<SqlExpression> columns, SqlExpression from, IReadOnlyCollection<IReadOnlyCollection<SqlExpression>> values)
        {
            Columns = columns;
            From = from;
            Values = values;
        }

        public override SqlExpressionType NodeType => SqlExpressionType.Insert;

        public IReadOnlyCollection<SqlExpression> Columns { get; }

        public SqlExpression From { get; }

        public IReadOnlyCollection<IReadOnlyCollection<SqlExpression>> Values { get; }

        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Insert(this);
        }
    }
}
