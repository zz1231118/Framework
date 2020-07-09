using System;
using System.Collections.Generic;
using Framework.Data.Expressions;

namespace Framework.Data.MsSql
{
    internal class MsSqlExpressionVisitor : SqlExpressionVisitor
    {
        public static SqlExpression LogicExpression(SqlExpression expression)
        {
            switch (expression)
            {
                case SqlUnaryExpression other:
                    if (other.Expression is SqlMemberExpression member)
                    {
                        return SqlExpression.Equal(member, SqlExpression.Constant(0));
                    }
                    break;
                case SqlMemberExpression _:
                    return SqlExpression.Equal(expression, SqlExpression.Constant(1));
            }

            return expression;
        }

        public static bool IsOrdinaryExpression(SqlExpression expression)
        {
            switch (expression)
            {
                case SqlMemberExpression _:
                case SqlConstantExpression _:
                case SqlFunctionExpression _:
                    return true;
                default:
                    return false;
            }
        }

        public override void Binary(SqlBinaryExpression node)
        {
            //Builder.Append("(");
            var logic = node.NodeType == SqlExpressionType.And || node.NodeType == SqlExpressionType.Or;
            var left = logic ? LogicExpression(node.Left) : node.Left;
            if (IsOrdinaryExpression(left))
            {
                left.Accept(this);
            }
            else
            {
                SqlBuilder.Append('(');
                left.Accept(this);
                SqlBuilder.Append(')');
            }
            SqlBuilder.Append(" ");
            switch (node.NodeType)
            {
                case SqlExpressionType.And:
                    SqlBuilder.Append("And");
                    break;
                case SqlExpressionType.Or:
                    SqlBuilder.Append("Or");
                    break;
                case SqlExpressionType.In:
                    SqlBuilder.Append("In");
                    break;
                case SqlExpressionType.Is:
                    SqlBuilder.Append("Is");
                    break;
                case SqlExpressionType.As:
                    SqlBuilder.Append("As");
                    break;

                case SqlExpressionType.GreaterThan:
                    SqlBuilder.Append(">");
                    break;
                case SqlExpressionType.GreaterThanOrEqual:
                    SqlBuilder.Append(">=");
                    break;
                case SqlExpressionType.Equal:
                    SqlBuilder.Append("=");
                    break;
                case SqlExpressionType.NotEqual:
                    SqlBuilder.Append("!=");
                    break;
                case SqlExpressionType.LessThanOrEqual:
                    SqlBuilder.Append("<=");
                    break;
                case SqlExpressionType.LessThan:
                    SqlBuilder.Append("<");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SqlBuilder.Append(" ");
            var right = logic ? LogicExpression(node.Right) : node.Right;
            right.Accept(this);
            //Builder.Append(")");
        }

        public override void Constant(SqlConstantExpression node)
        {
            if (node.Value == null)
            {
                SqlBuilder.Append("null");
            }
            else
            {
                var type = node.Value.GetType();
                var typeCode = Type.GetTypeCode(type);
                switch (typeCode)
                {
                    case TypeCode.DateTime:
                        SqlBuilder.Append("'").Append(((DateTime)node.Value).ToString("yyyy-MM-dd HH:mm:ss")).Append("'");
                        break;
                    case TypeCode.String:
                        SqlBuilder.Append("'").Append(node.Value).Append("'");
                        break;
                    default:
                        SqlBuilder.Append(node.Value);
                        break;
                }
            }
        }

        public override void Member(SqlMemberExpression node)
        {
            var name = MsSqlHelper.FormatSymbolName(node.Name);
            SqlBuilder.Append(name);
        }

        public override void Parameter(SqlParameterExpression node)
        {
            var name = MsSqlHelper.FormatParamName(node.Name);
            SqlBuilder.Append(name);
        }

        public override void Unary(SqlUnaryExpression node)
        {
            if (node.NodeType == SqlExpressionType.Not)
            {
                SqlBuilder.Append("Not ");
                var expre = LogicExpression(node.Expression);
                expre.Accept(this);
            }
            else if (node.NodeType == SqlExpressionType.Negate)
            {
                SqlBuilder.Append("-");
                node.Expression.Accept(this);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public override void NewArray(SqlNewArrayExpression node)
        {
            SqlBuilder.Append("(");
            using (var e = node.Items.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    e.Current.Accept(this);
                    while (e.MoveNext())
                    {
                        SqlBuilder.Append(", ");
                        e.Current.Accept(this);
                    }
                }
            }
            SqlBuilder.Append(")");
        }

        public override void Symbol(SqlSymbolExpression node)
        {
            SqlBuilder.Append(node.Token);
        }

        public override void Function(SqlFunctionExpression node)
        {
            SqlBuilder.Append(node.Name);
            SqlBuilder.Append("(");
            using (var e = node.Arguments.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    e.Current.Accept(this);
                    while (e.MoveNext())
                    {
                        SqlBuilder.Append(", ");
                    }
                }
            }
            SqlBuilder.Append(")");
        }

        public override void Select(SqlSelectLambdaExpression node)
        {
            SqlBuilder.Append("Select ");
            if (node.Top != null)
            {
                SqlBuilder.Append(node.Top.Value).Append(" ");
            }
            using (var e = node.Columns.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    e.Current.Accept(this);
                    while (e.MoveNext())
                    {
                        SqlBuilder.Append(", ");
                        e.Current.Accept(this);
                    }
                }
                else
                {
                    SqlBuilder.Append("* ");
                }
            }

            SqlBuilder.Append(" From ");
            node.From.Accept(this);
            if (node.Condition != null)
            {
                SqlBuilder.Append(" Where ");
                node.Condition.Accept(this);
            }
            if (node.Groups.Count > 0)
            {
                SqlBuilder.Append(" Group By ");
                using (var e = node.Groups.GetEnumerator())
                {
                    if (e.MoveNext())
                    {
                        e.Current.Accept(this);
                        while (e.MoveNext())
                        {
                            SqlBuilder.Append(", ");
                            e.Current.Accept(this);
                        }
                    }
                }
            }
            if (node.SortOrders.Count > 0)
            {
                SqlBuilder.Append(" Order By ");
                using (var e = node.SortOrders.GetEnumerator())
                {
                    if (e.MoveNext())
                    {
                        e.Current.Accept(this);
                        while (e.MoveNext())
                        {
                            SqlBuilder.Append(", ");
                            e.Current.Accept(this);
                        }
                    }
                }
            }
            if (node.Having != null)
            {
                SqlBuilder.Append(" Having ");
                node.Having.Accept(this);
            }
        }

        public override void Insert(SqlInsertLambdaExpression node)
        {
            SqlBuilder.Append("Insert Into ");
            node.From.Accept(this);
            if (node.Columns.Count > 0)
            {
                SqlBuilder.Append(" (");
                using (var e = node.Columns.GetEnumerator())
                {
                    e.MoveNext();
                    e.Current.Accept(this);
                    while (e.MoveNext())
                    {
                        SqlBuilder.Append(", ");
                        e.Current.Accept(this);
                    }
                }
                SqlBuilder.Append(')');
            }
            SqlBuilder.Append(" Values");
            void AcceptValues(IReadOnlyCollection<SqlExpression> values)
            {
                SqlBuilder.Append('(');
                using (var e = values.GetEnumerator())
                {
                    e.MoveNext();
                    e.Current.Accept(this);
                    while (e.MoveNext())
                    {
                        SqlBuilder.Append(", ");
                        e.Current.Accept(this);
                    }
                }
                SqlBuilder.Append(')');
            }
            using (var e = node.Values.GetEnumerator())
            {
                e.MoveNext();
                AcceptValues(e.Current);
                while (e.MoveNext())
                {
                    SqlBuilder.Append(", ");
                    AcceptValues(e.Current);
                }
            }
        }

        public override void Update(SqlUpdateLambdaExpression node)
        {
            SqlBuilder.Append("Update ");
            node.From.Accept(this);
            SqlBuilder.Append(" Set ");
            using (var e = node.Columns.GetEnumerator())
            {
                e.MoveNext();
                e.Current.Accept(this);
                while (e.MoveNext())
                {
                    SqlBuilder.Append(", ");
                    e.Current.Accept(this);
                }
            }
            SqlBuilder.Append(" From ");
            node.From.Accept(this);
            if (node.Condition != null)
            {
                SqlBuilder.Append(" Where ");
                node.Condition.Accept(this);
            }
        }

        public override void Delete(SqlDeleteLamdbaExpression node)
        {
            SqlBuilder.Append("Delete From ");
            node.From.Accept(this);
            if (node.Condition != null)
            {
                SqlBuilder.Append(" Where ");
                node.Condition.Accept(this);
            }
        }
    }
}
