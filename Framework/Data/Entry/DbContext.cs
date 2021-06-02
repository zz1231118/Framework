using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using Framework.Data.MsSql;

namespace Framework.Data.Entry
{
    /// <inheritdoc />
    public class DbContext : IQueryProvider, IDisposable
    {
        private readonly DbContextOptions options;
        private readonly Dictionary<Type, IInternalSet> nonGenericSets = new Dictionary<Type, IInternalSet>();
        private bool isDisposed;

        public DbContext(DbContextOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            this.options = options;
        }

        public DbContext()
            : this(DbContextOptions.Default)
        { }

        protected bool IsDisposed => isDisposed;

        public IReadOnlyCollection<ISet> Sets => nonGenericSets.Values;

        private Type GetColumnMappingType(ISchemaColumn column)
        {
            if (column.ConverterType != null)
            {
                var converter = EntityConverterManager.Gain(column.ConverterType);
                return converter.GetMappingType(column.PropertyType);
            }
            if (column.PropertyType.IsEnum)
            {
                return Enum.GetUnderlyingType(column.PropertyType);
            }
            var types = new Type[]
            {
                typeof(bool),
                typeof(byte),
                typeof(sbyte),
                typeof(short),
                typeof(ushort),
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(Guid),
            };
            if (types.Contains(column.PropertyType))
            {
                return column.PropertyType;
            }

            return typeof(string);
        }

        private DataTable GetSqlBulkTable(IEntitySchema entitySchema, RowEntry[] rowEntries)
        {
            Type mappingType;
            var dataTable = new DataTable();
            var columns = entitySchema.Columns.Where(p => !p.IsIdentity).ToArray();
            foreach (var column in columns)
            {
                mappingType = GetColumnMappingType(column);
                dataTable.Columns.Add(column.Name, mappingType);
            }

            object row;
            object? value;
            DataRow dataRow;
            foreach (var rowEntry in rowEntries)
            {
                row = rowEntry.Value;
                dataRow = dataTable.NewRow();
                foreach (var column in columns)
                {
                    value = column.GetValue(row);
                    dataRow[column.Name] = value ?? DBNull.Value;
                }

                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

        private string GetSqlBulkTargetCommandText(DbConnectionProvider connectionProvider, IEntitySchema entitySchema, string name)
        {
            var sb = new StringBuilder();
            sb.Append("Create Table ").Append(name).Append(" (").AppendLine();
            using (var e = entitySchema.Columns.Where(p => !p.IsIdentity).GetEnumerator())
            {
                e.MoveNext();
                ColumnFormatter(connectionProvider, e.Current, sb);
                while (e.MoveNext())
                {
                    sb.AppendLine(",");
                    ColumnFormatter(connectionProvider, e.Current, sb);
                }
            }

            sb.AppendLine();
            sb.AppendLine(");");
            return sb.ToString();

            static void ColumnFormatter(DbConnectionProvider provider, ISchemaColumn column, StringBuilder builder)
            {
                builder.AppendFormat("    {0} {1}", provider.NormalizeSymbol(column.Name), MsSqlHelper.GetDbTypeString(column));
                if (column.IsPrimary) builder.Append(" Not Null");
                else builder.Append(" Null");
            }
        }

        private string GetSqlBulkInsertCommandText(DbConnectionProvider connectionProvider, IEntitySchema entitySchema, string name)
        {
            var columns = entitySchema.Columns.Where(p => !p.IsIdentity).ToList();
            var sb = new StringBuilder();
            sb.AppendFormat("Insert Into [{0}] (", entitySchema.Name);
            using (var e = columns.GetEnumerator())
            {
                e.MoveNext();
                sb.AppendLine(connectionProvider.NormalizeSymbol(e.Current.Name));
                while (e.MoveNext())
                {
                    sb.AppendFormat(", {0}", connectionProvider.NormalizeSymbol(e.Current.Name));
                    sb.AppendLine();
                }
            }
            sb.Append(") Select ");
            using (var e = columns.GetEnumerator())
            {
                e.MoveNext();
                sb.AppendLine(connectionProvider.NormalizeSymbol(e.Current.Name));
                while (e.MoveNext())
                {
                    sb.AppendFormat(", {0}", connectionProvider.NormalizeSymbol(e.Current.Name));
                    sb.AppendLine();
                }
            }
            sb.AppendFormat("From {0};", name);
            return sb.ToString();
        }

        private string GetSqlBulkUpdateCommandText(DbConnectionProvider connectionProvider, IEntitySchema entitySchema, string name)
        {
            var columns = entitySchema.Columns.Where(p => !p.IsIdentity).ToList();
            var primaryColumn = columns.First(p => p.IsPrimary);
            var sb = new StringBuilder();
            sb.AppendFormat("Update [{0}] Set ", entitySchema.Name);
            columns = columns.Where(p => !p.IsPrimary).ToList();
            using (var e = columns.GetEnumerator())
            {
                e.MoveNext();
                sb.AppendFormat("[{0}].{1} = [Input].{1}", entitySchema.Name, connectionProvider.NormalizeSymbol(e.Current.Name)).AppendLine();
                while (e.MoveNext())
                {
                    sb.AppendFormat(", [{0}].{1} = [Input].{1}", entitySchema.Name, connectionProvider.NormalizeSymbol(e.Current.Name));
                    sb.AppendLine();
                }
            }
            sb.AppendFormat("From [{0}]", entitySchema.Name).AppendLine();
            sb.AppendFormat("Inner Join {0} As [Input] On [Input].{1} = [{2}].{1};", name, connectionProvider.NormalizeSymbol(primaryColumn.Name), entitySchema.Name);
            return sb.ToString();
        }

        private string GetSqlBulkDeleteCommandText(DbConnectionProvider connectionProvider, IEntitySchema entitySchema, string name)
        {
            var primaryColumn = entitySchema.Columns.First(p => p.IsPrimary);
            var sb = new StringBuilder();
            sb.AppendFormat("Delete [{0}]", entitySchema.Name).AppendLine();
            sb.AppendFormat("From [{0}]", entitySchema.Name).AppendLine();
            sb.AppendFormat("Inner Join {0} As [Input] On [Input].{1} = [{2}].{1};", name, connectionProvider.NormalizeSymbol(primaryColumn.Name), entitySchema.Name);
            return sb.ToString();
        }

        private void SqlBulkInsert(DbConnectionProvider connectionProvider, IEntitySchema entitySchema, RowEntry[] rowEntries, CancellationToken cancellationToken)
        {
            using (var dataTable = GetSqlBulkTable(entitySchema, rowEntries))
            {
                var dbConnection = connectionProvider.Allocate();
                try
                {
                    dbConnection.EnsureConnection();
                    var dbTransaction = dbConnection.DbConnection.BeginTransaction();
                    try
                    {
                        var input = string.Format("#{0}", Guid.NewGuid().ToString("N"));
                        using (var dbCommand = dbConnection.DbConnection.CreateCommand())
                        {
                            dbCommand.CommandText = GetSqlBulkTargetCommandText(connectionProvider, entitySchema, input);
                            dbCommand.CommandType = CommandType.Text;
                            dbCommand.CommandTimeout = checked((int)options.CommandTimeout.TotalSeconds);
                            dbCommand.Transaction = dbTransaction;
                            dbCommand.ExecuteNonQuery();
                        }
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        var bulkCopy = new SqlBulkCopy((SqlConnection)dbConnection.DbConnection, SqlBulkCopyOptions.Default, (SqlTransaction)dbTransaction);
                        bulkCopy.DestinationTableName = input;
                        bulkCopy.BulkCopyTimeout = checked((int)options.CommandTimeout.TotalSeconds);
                        bulkCopy.WriteToServer(dataTable);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        using (var dbCommand = dbConnection.DbConnection.CreateCommand())
                        {
                            dbCommand.CommandText = GetSqlBulkInsertCommandText(connectionProvider, entitySchema, input);
                            dbCommand.CommandType = CommandType.Text;
                            dbCommand.CommandTimeout = checked((int)options.CommandTimeout.TotalSeconds);
                            dbCommand.Transaction = dbTransaction;
                            dbCommand.ExecuteNonQuery();
                        }
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        dbTransaction.Commit();
                        foreach (var rowEntry in rowEntries)
                        {
                            rowEntry.AcceptChanges();
                        }
                    }
                    catch
                    {
                        dbTransaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        dbTransaction.Dispose();
                    }
                }
                finally
                {
                    connectionProvider.Release(dbConnection);
                }
            }
        }

        private void SqlBulkUpdate(DbConnectionProvider connectionProvider, IEntitySchema entitySchema, RowEntry[] rowEntries, CancellationToken cancellationToken)
        {
            var primaryColumns = entitySchema.Columns.Where(p => p.IsPrimary);
            if (primaryColumns.Count() != 1)
            {
                throw new InvalidConstraintException("only primary column not found.");
            }
            using (var dataTable = GetSqlBulkTable(entitySchema, rowEntries))
            {
                var dbConnection = connectionProvider.Allocate();
                try
                {
                    dbConnection.EnsureConnection();
                    var dbTransaction = dbConnection.DbConnection.BeginTransaction();
                    try
                    {
                        var input = string.Format("#{0}", Guid.NewGuid().ToString("N"));
                        using (var dbCommand = dbConnection.DbConnection.CreateCommand())
                        {
                            dbCommand.CommandText = GetSqlBulkTargetCommandText(connectionProvider, entitySchema, input);
                            dbCommand.CommandType = CommandType.Text;
                            dbCommand.CommandTimeout = checked((int)options.CommandTimeout.TotalSeconds);
                            dbCommand.Transaction = dbTransaction;
                            dbCommand.ExecuteNonQuery();
                        }
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        var bulkCopy = new SqlBulkCopy((SqlConnection)dbConnection.DbConnection, SqlBulkCopyOptions.Default, (SqlTransaction)dbTransaction);
                        bulkCopy.DestinationTableName = input;
                        bulkCopy.BulkCopyTimeout = checked((int)options.CommandTimeout.TotalSeconds);
                        bulkCopy.WriteToServer(dataTable);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        using (var dbCommand = dbConnection.DbConnection.CreateCommand())
                        {
                            dbCommand.CommandText = GetSqlBulkUpdateCommandText(connectionProvider, entitySchema, input);
                            dbCommand.CommandType = CommandType.Text;
                            dbCommand.CommandTimeout = checked((int)options.CommandTimeout.TotalSeconds);
                            dbCommand.Transaction = dbTransaction;
                            dbCommand.ExecuteNonQuery();
                        }
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        dbTransaction.Commit();
                        foreach (var rowEntry in rowEntries)
                        {
                            rowEntry.AcceptChanges();
                        }
                    }
                    catch
                    {
                        dbTransaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        dbTransaction.Dispose();
                    }
                }
                finally
                {
                    connectionProvider.Release(dbConnection);
                }
            }
        }

        private void SqlBulkDelete(DbConnectionProvider connectionProvider, IEntitySchema entitySchema, RowEntry[] rowEntries, CancellationToken cancellationToken)
        {
            var primaryColumns = entitySchema.Columns.Where(p => p.IsPrimary);
            if (primaryColumns.Count() != 1)
            {
                throw new InvalidConstraintException("only primary column not found.");
            }
            var primaryColumn = primaryColumns.First();
            var primaryParameterName = connectionProvider.NormalizeSymbol(primaryColumn.Name);
            using (var dataTable = new DataTable())
            {
                DataRow dataRow;
                Type mappingType = GetColumnMappingType(primaryColumn);
                dataTable.Columns.Add(primaryColumn.Name, mappingType);
                foreach (var rowEntry in rowEntries)
                {
                    dataRow = dataTable.NewRow();
                    dataRow[primaryColumn.Name] = primaryColumn.GetValue(rowEntry.Value);
                    dataTable.Rows.Add(dataRow);
                }

                var dbConnection = connectionProvider.Allocate();
                try
                {
                    dbConnection.EnsureConnection();
                    var dbTransaction = dbConnection.DbConnection.BeginTransaction();
                    try
                    {
                        var input = string.Format("#{0}", Guid.NewGuid().ToString("N"));
                        using (var dbCommand = dbConnection.DbConnection.CreateCommand())
                        {
                            dbCommand.CommandText = string.Format("Create Table {0} ({1} [{2}] Not Null);", input, primaryParameterName, MsSqlHelper.GetDbTypeString(primaryColumn));
                            dbCommand.CommandType = CommandType.Text;
                            dbCommand.CommandTimeout = checked((int)options.CommandTimeout.TotalSeconds);
                            dbCommand.Transaction = dbTransaction;
                            dbCommand.ExecuteNonQuery();
                        }
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        var bulkCopy = new SqlBulkCopy((SqlConnection)dbConnection.DbConnection, SqlBulkCopyOptions.Default, (SqlTransaction)dbTransaction);
                        bulkCopy.DestinationTableName = input;
                        bulkCopy.BulkCopyTimeout = checked((int)options.CommandTimeout.TotalSeconds);
                        bulkCopy.WriteToServer(dataTable);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        using (var dbCommand = dbConnection.DbConnection.CreateCommand())
                        {
                            dbCommand.CommandText = GetSqlBulkDeleteCommandText(connectionProvider, entitySchema, input);
                            dbCommand.CommandType = CommandType.Text;
                            dbCommand.CommandTimeout = checked((int)options.CommandTimeout.TotalSeconds);
                            dbCommand.Transaction = dbTransaction;
                            dbCommand.ExecuteNonQuery();
                        }
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        dbTransaction.Commit();
                        foreach (var rowEntry in rowEntries)
                        {
                            rowEntry.AcceptChanges();
                        }
                    }
                    catch
                    {
                        dbTransaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        dbTransaction.Dispose();
                    }
                }
                finally
                {
                    connectionProvider.Release(dbConnection);
                }
            }
        }

        protected void CheckDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;
            }
        }

        public IDbSet Set(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            CheckDisposed();
            if (!nonGenericSets.TryGetValue(entityType, out IInternalSet dbSet))
            {
                var entitySchema = EntitySchemaManager.GetSchema(entityType, true);
                var arguments = new object[] { this, entityType, entitySchema };
                var argumentTypes = new Type[] { typeof(DbContext), typeof(Type), typeof(IEntitySchema) };
                var dbSetType = typeof(DbSet<>).MakeGenericType(entityType);
                var constructor = dbSetType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, argumentTypes, null);
                dbSet = (IInternalSet)constructor.Invoke(arguments);
                nonGenericSets[entityType] = dbSet;
            }

            return (IDbSet)dbSet;
        }

        public IDbSet<T> Set<T>()
            where T : class
        {
            return (IDbSet<T>)Set(typeof(T));
        }

        public IQueryable<T> CreateQuery<T>()
        {
            return new ObjectQuery<T>(this);
        }

        public void Clear()
        {
            nonGenericSets.Clear();
        }

        public void SaveChanges(CancellationToken cancellationToken)
        {
            CheckDisposed();
            if (cancellationToken.IsCancellationRequested)
                return;
            if (nonGenericSets.Count == 0)
                return;

            RowEntry[] rowEntries;
            List<Exception>? exceptions = null;
            DbConnectionProvider connectionProvider;
            foreach (var dbSet in nonGenericSets.Values)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                if (dbSet.Count == 0)
                {
                    continue;
                }

                rowEntries = dbSet.RowEntries.ToArray();
                connectionProvider = DbConnectionManager.Gain(dbSet.EntitySchema.ConnectKey, true);
                foreach (var rowEntryGroup in rowEntries.GroupBy(p => p.State))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    try
                    {
                        switch (rowEntryGroup.Key)
                        {
                            case EntityState.Detached:
                            case EntityState.Unchanged:
                                break;
                            case EntityState.Added:
                                rowEntries = rowEntryGroup.ToArray();
                                SqlBulkInsert(connectionProvider, dbSet.EntitySchema, rowEntries, cancellationToken);
                                break;
                            case EntityState.Deleted:
                                rowEntries = rowEntryGroup.ToArray();
                                SqlBulkDelete(connectionProvider, dbSet.EntitySchema, rowEntries, cancellationToken);
                                break;
                            case EntityState.Modified:
                                rowEntries = rowEntryGroup.ToArray();
                                SqlBulkUpdate(connectionProvider, dbSet.EntitySchema, rowEntries, cancellationToken);
                                break;
                            default:
                                throw new InvalidOperationException($"unknown EntityState: {rowEntryGroup.Key}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        switch (options.ExceptionHandling)
                        {
                            case ExceptionHandling.Interrupt:
                                throw;
                            case ExceptionHandling.Skip:
                                exceptions ??= new List<Exception>();
                                exceptions.Add(ex);
                                break;
                        }
                    }
                }
            }

            if (exceptions?.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        public void SaveChanges()
        {
            SaveChanges(CancellationToken.None);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            var elementType = TypeSystem.GetElementType(expression.Type);
            var objectQueryType = typeof(ObjectQuery<>).MakeGenericType(elementType);
            return (IQueryable)Activator.CreateInstance(objectQueryType, new object[] { this, expression });
        }

        IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
        {
            return new ObjectQuery<TElement>(this, expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}
