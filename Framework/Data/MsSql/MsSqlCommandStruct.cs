using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Framework.Data.Command;
using Framework.Data.Expressions;
using Framework.Linq;

namespace Framework.Data.MsSql
{
    internal class MsSqlCommandStruct : DbCommandStruct
    {
        public MsSqlCommandStruct(string name, DbCommandMode mode, IEnumerable<SqlExpression>? columns = null)
            : base(name, mode, columns)
        { }

        protected sealed override string OnParseSelect()
        {
            var builder = new StringBuilder("Select ");
            if (Top != null)
            {
                builder.AppendFormat("Top {0} ", Top.Value);
            }
            if (Columns.Count == 0)
            {
                builder.AppendLine("*");
            }
            else
            {
                using (var e = Columns.GetEnumerator())
                {
                    e.MoveNext();
                    var visitor = new MsSqlExpressionVisitor();
                    e.Current.Accept(visitor);
                    builder.AppendFormat("{0}", visitor.Complier()).AppendLine();
                    while (e.MoveNext())
                    {
                        visitor.Reset();
                        e.Current.Accept(visitor);

                        builder.AppendFormat(", {0}", visitor.Complier()).AppendLine();
                    }
                }
            }
            builder.AppendFormat("From {0}", MsSqlHelper.FormatSymbolName(Name));
            if (Condition != null)
            {
                builder.AppendLine();
                builder.Append("Where ");

                var visitor = new MsSqlExpressionVisitor();
                var expre = MsSqlExpressionVisitor.LogicExpression(Condition);
                expre.Accept(visitor);
                builder.Append(visitor.Complier());
            }
            if (Groups != null)
            {
                builder.AppendLine();
                builder.AppendFormat("Group By {0}", string.Join(", ", Groups.Select(p => string.Format("[{0}]", p.Name))));
            }
            if (SortOrders != null)
            {
                using (var e = SortOrders.GetEnumerator())
                {
                    if (e.MoveNext())
                    {
                        builder.AppendLine();
                        builder.AppendFormat("Order By [{0}] {1}", e.Current.Member.Name, e.Current.Ascending ? "Asc" : "Desc");
                        while (e.MoveNext())
                        {
                            builder.Append(", ");
                            builder.AppendFormat("[{0}] {1}", e.Current.Member.Name, e.Current.Ascending ? "Asc" : "Desc");
                        }
                    }
                }
            }
            if (RowOffset != null)
            {
                //offset 5*2 rows fetch next 5 rows only
                builder.AppendLine();
                builder.Append("Offset ").Append(RowOffset.Offset).Append(" Rows Fetch Next ").Append(RowOffset.Count).Append(" Rows Only");
            }
            return builder.ToString();
        }

        protected sealed override string OnParseUpdate()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Update {0}", MsSqlHelper.FormatSymbolName(Name)).AppendLine();
            var retriever = new SqlMemberExpressionRetriever();
            foreach (var column in Columns)
            {
                column.Accept(retriever);
            }
            using (var e = retriever.Members.GetEnumerator())
            {
                if (!e.MoveNext())
                    throw new InvalidOperationException("parameter count is zero");

                builder.AppendFormat("Set [{0}] = @{0}", e.Current.Name).AppendLine();
                while (e.MoveNext())
                {
                    builder.AppendFormat(", [{0}] = @{0}", e.Current.Name).AppendLine();
                }
            }
            builder.AppendFormat("From {0}", MsSqlHelper.FormatSymbolName(Name));
            //foreach (var inner in _inners)
            //{
            //    builder.AppendLine();
            //    inner.Parser();
            //    builder.Append(inner.Sql);
            //}
            if (Condition != null)
            {
                builder.AppendLine();
                builder.Append("Where ");
                var visitor = new MsSqlExpressionVisitor();
                var expre = MsSqlExpressionVisitor.LogicExpression(Condition);
                expre.Accept(visitor);
                builder.Append(visitor.Complier());
            }

            return builder.ToString();
        }

        protected sealed override string OnParseInsert()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Insert Into {0} (", MsSqlHelper.FormatSymbolName(Name));
            var retriever = new SqlMemberExpressionRetriever();
            foreach (var column in Columns)
            {
                column.Accept(retriever);
            }
            using (var e = retriever.Members.GetEnumerator())
            {
                if (Parameters.Count == 0)
                    throw new InvalidOperationException("parameter count is zero");

                if (e.MoveNext())
                {
                    builder.AppendFormat("[{0}]", e.Current.Name);
                    while (e.MoveNext())
                    {
                        builder.AppendLine();
                        builder.AppendFormat(", [{0}]", e.Current.Name);
                    }
                }
            }

            builder.Append(") Values (");
            using (var e = retriever.Members.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    builder.AppendFormat("@{0}", e.Current.Name);
                    while (e.MoveNext())
                    {
                        builder.AppendLine();
                        builder.AppendFormat(", @{0}", e.Current.Name);
                    }
                }
            }

            builder.AppendFormat(")");
            return builder.ToString();
        }

        protected sealed override string OnParseDelete()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Delete From {0}", MsSqlHelper.FormatSymbolName(Name));
            //foreach (var inner in _inners)
            //{
            //    builder.AppendLine();
            //    inner.Parser();
            //    builder.Append(inner.Sql);
            //}
            if (Condition != null)
            {
                builder.AppendLine();
                builder.Append("Where ");
                var visitor = new MsSqlExpressionVisitor();
                var expre = MsSqlExpressionVisitor.LogicExpression(Condition);
                expre.Accept(visitor);
                builder.Append(visitor.Complier());
            }

            return builder.ToString();
        }

        public override void AddParameter(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            name = MsSqlHelper.FormatParamName(name);
            AddParameter(new SqlParameter(name, value ?? DBNull.Value));
        }
    }

    internal class MsSqlDbCommandStruct<T> : MsSqlCommandStruct, IDbCommandStruct<T>
    {
        public MsSqlDbCommandStruct(string name, DbCommandMode mode, IEnumerable<SqlExpression>? columns = null)
            : base(name, mode, columns)
        { }

        public void GroupBy(Expression<Func<T, object>> expression)
        {
            this.GroupBy<T>(expression);
        }

        public void OrderBy(Expression<Func<T, object>> expression, bool ascending = true)
        {
            this.OrderBy<T>(expression, ascending);
        }

        public void Where(Expression<Func<T, bool>> expression)
        {
            this.Where<T>(expression);
        }
    }
}
