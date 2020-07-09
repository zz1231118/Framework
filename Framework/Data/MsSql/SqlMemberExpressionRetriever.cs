using System;
using System.Collections.Generic;
using System.Text;
using Framework.Data.Expressions;

namespace Framework.Data.MsSql
{
    internal class SqlMemberExpressionRetriever : MsSqlExpressionVisitor
    {
        private readonly List<SqlMemberExpression> members = new List<SqlMemberExpression>();

        public IReadOnlyCollection<SqlMemberExpression> Members => members;

        public override void Member(SqlMemberExpression node)
        {
            base.Member(node);
            members.Add(node);
        }
    }
}
