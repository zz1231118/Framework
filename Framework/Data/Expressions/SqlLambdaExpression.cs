using System;

namespace Framework.Data.Expressions
{
    class SqlLambdaExpression : SqlExpression
    {
        public override SqlExpressionType NodeType => throw new NotImplementedException();

        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}
