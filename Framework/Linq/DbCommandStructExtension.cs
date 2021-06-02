using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Framework.Data.Command;
using Framework.Data.Expressions;

namespace Framework.Linq
{
    public static class DbCommandStructExtension
    {
        private static SqlExpression Accept(IDbCommandStruct @struct, Expression expression, ref int pIndex)
        {
            if (expression is BinaryExpression binary)
            {
                SqlExpression lexpre = Accept(@struct, binary.Left, ref pIndex);
                SqlExpression rexpre = Accept(@struct, binary.Right, ref pIndex);
                switch (binary.NodeType)
                {
                    case ExpressionType.AndAlso:
                        return SqlExpression.And(lexpre, rexpre);
                    case ExpressionType.OrElse:
                        return SqlExpression.Or(lexpre, rexpre);

                    case ExpressionType.GreaterThan:
                        return SqlExpression.GreaterThan(lexpre, rexpre);
                    case ExpressionType.GreaterThanOrEqual:
                        return SqlExpression.GreaterThanOrEqual(lexpre, rexpre);
                    case ExpressionType.Equal:
                        switch (rexpre)
                        {
                            case SqlConstantExpression constant when constant.Value is null:
                                return SqlExpression.Is(lexpre, rexpre);
                            default:
                                return SqlExpression.Equal(lexpre, rexpre);
                        }
                    case ExpressionType.NotEqual:
                        switch (rexpre)
                        {
                            case SqlConstantExpression constant when constant.Value is null:
                                return SqlExpression.Not(SqlExpression.Is(lexpre, rexpre));
                            default:
                                return SqlExpression.NotEqual(lexpre, rexpre);
                        }
                    case ExpressionType.LessThanOrEqual:
                        return SqlExpression.LessThanOrEqual(lexpre, rexpre);
                    case ExpressionType.LessThan:
                        return SqlExpression.LessThan(lexpre, rexpre);
                    default:
                        throw new ArgumentException(nameof(binary.NodeType));
                }
            }
            else if (expression is MemberExpression member)
            {
                if (member.Expression.NodeType == ExpressionType.Parameter)
                {
                    return SqlExpression.Member(member.Member.Name);
                }
                else
                {
                    var name = "var" + pIndex++;
                    var parent = member.Expression?.GetValue();
                    var value = member.Member.GetValue(parent);
                    @struct.AddParameter(name, value);
                    return SqlExpression.Paramter(name);
                }
            }
            else if (expression is UnaryExpression unary)
            {
                var node = Accept(@struct, unary.Operand, ref pIndex);
                if (unary.NodeType == ExpressionType.Not)
                {
                    return SqlExpression.Not(node);
                }
                else if (unary.NodeType == ExpressionType.Negate)
                {
                    return SqlExpression.Negate(node);
                }
                else if (unary.NodeType == ExpressionType.Convert)
                {
                    return node;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            else if (expression is ConstantExpression constant)
            {
                if (constant.Value == null)
                {
                    return SqlExpression.Constant(null);
                }

                var name = "var" + pIndex++;
                @struct.AddParameter(name, constant.Value);
                return SqlExpression.Paramter(name);
            }
            else if (expression is NewExpression newexpr)
            {
                var objParams = new object[newexpr.Arguments.Count];
                for (int i = 0; i < objParams.Length; i++)
                    objParams[i] = newexpr.Arguments[i].GetValue();

                var obj = newexpr.Constructor.Invoke(objParams);
                return SqlExpression.Constant(obj);
            }
            else if (expression is MethodCallExpression methodCall)
            {
                if (methodCall.Method.Name == "Contains" && methodCall.Method.ReturnType == typeof(bool) && methodCall.Arguments.Count == 1)
                {
                    var parameters = methodCall.Method.GetParameters();
                    if (parameters.Length == 1)
                    {
                        var collection = GetValue(methodCall.Object) as IEnumerable;
                        var expressions = new List<SqlExpression>();
                        foreach (var value in collection)
                        {
                            expressions.Add(SqlExpression.Constant(value));
                        }

                        int inIndex = 0;
                        var argument = Accept(@struct, methodCall.Arguments[0], ref inIndex);
                        var newArray = SqlExpression.NewArray(expressions);
                        return SqlExpression.In(argument, newArray);
                    }
                }
            }

            throw new ArgumentException();
        }

        private static object GetValue(this Expression expression)
        {
            if (expression is ConstantExpression constant)
            {
                return constant.Value;
            }
            else if (expression is MemberExpression member)
            {
                var parent = member.Expression?.GetValue();
                return member.Member.GetValue(parent);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private static object GetValue(this System.Reflection.MemberInfo member, object obj, object[] index = null)
        {
            switch (member)
            {
                case System.Reflection.FieldInfo field: return field.GetValue(obj);
                case System.Reflection.PropertyInfo property: return property.GetValue(obj, index);
                default: throw new ArgumentException($"unknown member type:{member.GetType().FullName}");
            }
        }

        public static void Where<T>(this IDbCommandStruct @struct, Expression<Func<T, bool>> expression)
        {
            if (@struct == null)
                throw new ArgumentNullException(nameof(@struct));
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            int pIndex = 0;
            @struct.Condition = Accept(@struct, expression.Body, ref pIndex);
        }

        public static void OrderBy<T>(this IDbCommandStruct @struct, Expression<Func<T, object>> expression, bool ascending = true)
        {
            if (@struct == null)
                throw new ArgumentNullException(nameof(@struct));
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            MemberExpression memberExpression;
            switch (expression.Body)
            {
                case MemberExpression other:
                    memberExpression = other;
                    break;
                case UnaryExpression other:
                    if (other.NodeType == ExpressionType.Convert && other.Operand is MemberExpression m)
                    {
                        memberExpression = m;
                        break;
                    }
                    throw new InvalidOperationException("invalid expression");
                default:
                    throw new InvalidOperationException("invalid expression");
            }

            var sortOrders = @struct.SortOrders?.ToList() ?? new List<IDbSortClause>();
            sortOrders.Add(new DbSortClause(SqlExpression.Member(memberExpression.Member.Name), ascending));
            @struct.SortOrders = sortOrders.ToArray();
        }

        public static void GroupBy<T>(this IDbCommandStruct @struct, Expression<Func<T, object>> expression)
        {
            if (@struct == null)
                throw new ArgumentNullException(nameof(@struct));
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            MemberExpression memberExpression;
            switch (expression.Body)
            {
                case MemberExpression other:
                    memberExpression = other;
                    break;
                case UnaryExpression other:
                    if (other.NodeType == ExpressionType.Convert && other.Operand is MemberExpression m)
                    {
                        memberExpression = m;
                        break;
                    }
                    throw new InvalidOperationException("invalid expression");
                default:
                    throw new InvalidOperationException("invalid expression");
            }

            var groups = @struct.Groups?.ToList() ?? new List<SqlMemberExpression>();
            groups.Add(SqlExpression.Member(memberExpression.Member.Name));
            @struct.Groups = groups.ToArray();
        }

        struct DbSortClause : IDbSortClause
        {
            public DbSortClause(SqlMemberExpression member, bool ascending)
            {
                Member = member;
                Ascending = ascending;
            }

            public SqlMemberExpression Member { get; }

            public bool Ascending { get; }
        }
    }
}
