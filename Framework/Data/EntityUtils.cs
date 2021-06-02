using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Framework.Data.MsSql;
using Framework.Linq;

namespace Framework.Data
{
    public static class EntityUtils
    {
        public static DataTable CreateDataTable(IEntitySchema view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            DataTable table = new DataTable();
            foreach (var column in view.Columns.OrderByDescending(p => p.IsPrimary).ThenBy(p => p.Order))
            {
                if (column.Mode == ColumnMode.ReadOnly || column.IsIdentity)
                    continue;

                var tType = ConvertToType(column.PropertyType);
                table.Columns.Add(column.Name, tType);
            }
            return table;
        }

        public static void AddRowToTable(DataTable table, object obj, IEntitySchema? view = null)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (view == null)
            {
                var type = obj.GetType();
                if (!EntitySchemaManager.TryGetSchema(type, out view))
                {
                    view = EntitySchemaManager.LoadEntity(type);
                }
            }
            var row = table.NewRow();
            foreach (var column in view.Columns)
            {
                if (column.Mode == ColumnMode.ReadOnly)
                    continue;

                var value = column.GetValue(obj);
                row[column.Name] = value;
            }
            table.Rows.Add(row);
        }

        public static Type ConvertToType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type == typeof(byte) || type == typeof(sbyte) ||
                type == typeof(short) || type == typeof(ushort) ||
                type == typeof(int) || type == typeof(uint) ||
                type == typeof(long) || type == typeof(ulong) ||
                type == typeof(float) || type == typeof(double) ||
                type == typeof(bool) || type == typeof(Guid) ||
                type == typeof(DateTime) || type.IsEnum)
            {
                return type;
            }

            return typeof(string);
        }

        public static SqlDbType ConvertToSqlType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type == typeof(byte))
                return SqlDbType.TinyInt;
            if (type == typeof(sbyte))
                return SqlDbType.TinyInt;
            if (type == typeof(short))
                return SqlDbType.SmallInt;
            if (type == typeof(ushort))
                return SqlDbType.SmallInt;
            if (type == typeof(int))
                return SqlDbType.Int;
            if (type == typeof(uint))
                return SqlDbType.Int;
            if (type == typeof(long))
                return SqlDbType.BigInt;
            if (type == typeof(ulong))
                return SqlDbType.BigInt;
            if (type == typeof(float))
                return SqlDbType.Real;
            if (type == typeof(double))
                return SqlDbType.Float;
            if (type == typeof(bool))
                return SqlDbType.Bit;
            if (type == typeof(byte[]))
                return SqlDbType.Binary;
            if (type == typeof(string))
                return SqlDbType.NVarChar;
            if (type == typeof(decimal))
                return SqlDbType.Decimal;
            if (type == typeof(DateTime))
                return SqlDbType.DateTime;
            if (type == typeof(Guid))
                return SqlDbType.UniqueIdentifier;

            throw new Exception("[" + type.Name + "] can't change to SqlType");
        }

        public static string ConvertToDbType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type == typeof(byte))
                return SqlDbType.TinyInt.ToString();
            if (type == typeof(sbyte))
                return SqlDbType.TinyInt.ToString();
            if (type == typeof(short))
                return SqlDbType.SmallInt.ToString();
            if (type == typeof(ushort))
                return SqlDbType.SmallInt.ToString();
            if (type == typeof(int))
                return SqlDbType.Int.ToString();
            if (type == typeof(uint))
                return SqlDbType.Int.ToString();
            if (type == typeof(long))
                return SqlDbType.BigInt.ToString();
            if (type == typeof(ulong))
                return SqlDbType.BigInt.ToString();
            if (type == typeof(float))
                return SqlDbType.Real.ToString();
            if (type == typeof(double))
                return SqlDbType.Float.ToString();
            if (type == typeof(bool))
                return SqlDbType.Bit.ToString();
            if (type == typeof(byte[]))
                return SqlDbType.Binary.ToString();
            if (type == typeof(string))
                return SqlDbType.NVarChar.ToString();
            if (type == typeof(decimal))
                return SqlDbType.Decimal.ToString();
            if (type == typeof(DateTime))
                return SqlDbType.DateTime.ToString();
            if (type == typeof(Guid))
                return SqlDbType.UniqueIdentifier.ToString();
            if (type.IsEnum)
                return ConvertToDbType(type.GetEnumUnderlyingType());
            if (type == typeof(TimeSpan))
                return string.Format("{0}({1})", SqlDbType.VarChar.ToString(), 15);

            throw new ArgumentException("[" + type.Name + "] can't change to SqlType");
        }

        public static string GetCreateTableCommand(ISchemaTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var columns = table.Columns;
            if (columns.Count(p => p.IsPrimary) > 1)
                throw new ArgumentException("primary count > 1");

            var builder = new StringBuilder();
            builder.AppendFormat("Create Table [dbo].[{0}] (", table.Name);
            builder.AppendLine();
            using (var e = columns.OrderByDescending(p => p.IsPrimary).ThenBy(p => p.Order).GetEnumerator())
            {
                if (e.MoveNext())
                {
                    builder.Append(GetColumnCommand(table, e.Current));
                    while (e.MoveNext())
                    {
                        builder.AppendLine();
                        builder.Append(",");
                        builder.Append(GetColumnCommand(table, e.Current));
                    }
                    builder.AppendLine();
                }
            }
            var primaryColumn = columns.FirstOrDefault(p => p.IsPrimary);
            if (primaryColumn != null)
            {
                builder.AppendFormat(" Constraint [PK_{0}] Primary Key Clustered ", table.Name);
                builder.AppendLine();
                builder.AppendLine(" (");
                builder.AppendFormat("   [{0}] Asc", primaryColumn.Name);
                builder.AppendLine();
                builder.AppendLine(" ) With (Pad_Index = Off, Statistics_Norecompute = Off, Ignore_Dup_Key = Off, Allow_Row_Locks = On, Allow_Page_Locks = On) On [Primary]");
            }
            builder.AppendLine(") On [Primary]");
            return builder.ToString();
        }
        private static string GetColumnCommand(ISchemaTable table, ISchemaColumn column)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("[{0}] {1}", column.Name, MsSqlHelper.GetDbTypeString(column));
            if (column.IsIdentity)
            {
                builder.AppendFormat(" Identity({0}, {1})", column.IdentitySeed, column.Increment);
            }
            if (column.IsPrimary || !column.IsNullable)
            {
                builder.Append(" Not");
            }
            builder.Append(" Null");
            if (!string.IsNullOrEmpty(column.DefaultValue))
            {
                builder.AppendFormat(" Constraint [DF_{0}_{1}] Default ({2})", table.Name, column.Name, column.DefaultValue);
            }
            return builder.ToString();
        }
        public static string GetCreateTypeCommand(IEntitySchema view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var columns = view.Columns;
            if (columns.Count(p => p.IsPrimary) != 1)
                throw new ArgumentException("primary error!");

            var builder = new StringBuilder();
            builder.AppendFormat("Create Type [dbo].[{0}Type] As Table", view.Name);
            builder.AppendLine();
            builder.AppendLine("(");

            foreach (var column in columns.OrderByDescending(p => p.IsPrimary).ThenBy(p => p.Order))
            {
                if (column.Mode == ColumnMode.ReadOnly)
                    continue;

                builder.AppendFormat("  [{0}] {1}", column.Name, MsSqlHelper.GetDbTypeString(column));
                if (column.IsPrimary || !column.IsNullable)
                    builder.Append(" Not");
                builder.AppendLine(" Null,");
            }
            var primaryColumn = columns.First(p => p.IsPrimary);
            builder.AppendFormat("  Primary Key ([{0}])", primaryColumn.Name);
            builder.AppendLine();
            builder.AppendLine(")");
            return builder.ToString();
        }
        public static string GetViewCommand(IEntitySchema view, ProcedureOperate operate)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var columns = view.Columns;
            if (columns.Count == 0)
                throw new ArgumentException("not column");
            if (columns.Count(p => p.IsPrimary) != 1)
                throw new ArgumentException("primary error!");

            var groupColumnList = columns.GroupBy(p => p.Table).OrderByDescending(p => p.Any(t => t.IsPrimary)).ToList();
            if (groupColumnList.Count <= 1)
                throw new ArgumentException("table count <= 1");

            var primaryColumn = columns.First(p => p.IsPrimary);
            var builder = new StringBuilder();
            builder.AppendFormat("{0} View [dbo].[v_{1}]", operate.ToString(), view.Name);
            builder.AppendLine();
            builder.AppendLine("As");
            var fristGroupColumn = groupColumnList.First();
            builder.Append("Select ");
            using (var ce = groupColumnList.GetEnumerator())
            {
                ce.MoveNext();
                using (var e = ce.Current.GetEnumerator())
                {
                    e.MoveNext();
                    builder.AppendFormat("[dbo].[{0}].[{1}]", ce.Current.Key, e.Current.Name);
                    while (e.MoveNext())
                        builder.AppendLine().AppendFormat(", [dbo].[{0}].[{1}]", ce.Current.Key, e.Current.Name);
                }
                while (ce.MoveNext())
                {
                    using (var e = ce.Current.GetEnumerator())
                    {
                        while (e.MoveNext())
                            builder.AppendLine().AppendFormat(", [dbo].[{0}].[{1}]", ce.Current.Key, e.Current.Name);
                    }
                }
            }
            builder.AppendLine();
            builder.AppendFormat("From [dbo].[{0}]", fristGroupColumn.Key);
            for (int i = 1; i < groupColumnList.Count; i++)
            {
                var groupColumn = groupColumnList[i];
                builder.AppendLine();
                builder.AppendFormat("Inner Join [dbo].[{0}] On [{0}].[{1}] = [{2}].[{1}]",
                    groupColumn.Key, primaryColumn.Name, fristGroupColumn.Key);
            }

            return builder.ToString();
        }

        public static string GetCreateProcedureCommand(IEntitySchema view, ProcedureUsage usage)
        {
            if (view == null)
                throw new ArgumentNullException("table");

            var columns = view.Columns;
            if (columns.Count(p => p.IsPrimary) > 1)
                throw new ArgumentException("primary count > 1");

            return GetProcedureCommand(view, ProcedureOperate.Create, usage);
        }
        private static string GetProcedureCommand(IEntitySchema view, ProcedureOperate operate, ProcedureUsage usage)
        {
            switch (usage)
            {
                case ProcedureUsage.Insert:
                    return GetProcedureInsertCommand(operate, view);
                case ProcedureUsage.Update:
                    return GetProcedureUpdateCommand(operate, view);
                case ProcedureUsage.TypeInsert:
                    return GetProcedureTypeInsertCommand(operate, view);
                case ProcedureUsage.TypeUpdate:
                    return GetProcedureTypeUpdateCommand(operate, view);
                default:
                    throw new InvalidOperationException("unknown procedure type!");
            }
        }
        private static string GetProcedureInsertCommand(ProcedureOperate operate, IEntitySchema view)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0} Procedure [dbo].[Insert{1}]", operate.ToString(), view.Name);
            builder.AppendLine();
            var viewColumns = view.Columns;
            var viewColumnList = viewColumns.Where(p => p.Mode != ColumnMode.ReadOnly && !p.IsIdentity).ToList();
            if (viewColumnList.Count > 1)
            {
                var firstColumn = viewColumnList.First();
                builder.AppendFormat("  @{0} {1}", firstColumn.Name, MsSqlHelper.GetDbTypeString(firstColumn));
                if (!string.IsNullOrEmpty(firstColumn.DefaultValue))
                    builder.AppendFormat(" = {0}", firstColumn.DefaultValue);
            }
            for (int i = 1; i < viewColumnList.Count; i++)
            {
                var column = viewColumnList[i];
                builder.AppendLine(",");
                builder.AppendFormat("  @{0} {1}", column.Name, MsSqlHelper.GetDbTypeString(column));
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    builder.AppendFormat(" = {0}", column.DefaultValue);
            }
            builder.AppendLine();
            builder.AppendLine("As");
            builder.AppendLine("Begin");
            builder.AppendLine("  Set NoCount ON;");
            builder.AppendLine();

            var builderParam = new StringBuilder();
            var builderColumn = new StringBuilder();
            foreach (var table in view.Tables)
            {
                var aryTableColumn = table.Columns;
                var tableColumnList = aryTableColumn.Where(p => p.CanRead && p.Mode != ColumnMode.ReadOnly && !p.IsIdentity).ToList();
                if (tableColumnList.Count > 0)
                {
                    var firstColumn = tableColumnList.First();
                    builderColumn.AppendFormat("[{0}]", firstColumn.Name);
                    builderParam.AppendFormat("@{0}", firstColumn.Name);
                }
                for (int i = 1; i < tableColumnList.Count; i++)
                {
                    var column = tableColumnList[i];
                    builderColumn.AppendLine();
                    builderColumn.AppendFormat("  ,[{0}]", column.Name);

                    builderParam.AppendLine();
                    builderParam.AppendFormat("  ,@{0}", column.Name);
                }
                builder.AppendFormat("  Insert Into [dbo].[{0}]", table.Name);
                builder.AppendLine();
                builder.Append("  (");
                builder.AppendLine(builderColumn.ToString());
                builder.AppendLine("  ) Values");
                builder.Append("  (");
                builder.AppendLine(builderParam.ToString());
                builder.AppendLine("  );");
                builder.AppendLine();
                builderColumn.Clear();
                builderParam.Clear();
            }

            builder.AppendLine("End");
            return builder.ToString();
        }
        private static string GetProcedureUpdateCommand(ProcedureOperate operate, IEntitySchema view)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0} Procedure [dbo].[Update{1}]", operate.ToString(), view.Name);
            builder.AppendLine();
            var viewColumns = view.Columns;
            var validColumns = viewColumns.Where(p => p.Mode != ColumnMode.ReadOnly).ToList();
            if (validColumns.Count(p => p.IsPrimary) != 1)
            {
                throw new ArgumentException("view primary error!");
            }

            var primaryColumn = validColumns.First(p => p.IsPrimary);
            validColumns.RemoveAll(p => !p.IsPrimary && p.IsIdentity);
            if (validColumns.Count > 1)
            {
                var firstColumn = validColumns.First();
                builder.AppendFormat("  @{0} {1}", firstColumn.Name, MsSqlHelper.GetDbTypeString(firstColumn));
                if (!string.IsNullOrEmpty(firstColumn.DefaultValue))
                    builder.AppendFormat(" = {0}", firstColumn.DefaultValue);
            }
            for (int i = 1; i < validColumns.Count; i++)
            {
                var column = validColumns[i];
                builder.AppendLine(",");
                builder.AppendFormat("  @{0} {1}", column.Name, MsSqlHelper.GetDbTypeString(column));
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    builder.AppendFormat(" = {0}", column.DefaultValue);
            }
            builder.AppendLine();
            builder.AppendLine("As");
            builder.AppendLine("Begin");
            builder.AppendLine("  Set NoCount ON;");
            builder.AppendLine();

            var builderColumn = new StringBuilder();
            validColumns.Remove(primaryColumn);
            var groupColumnList = validColumns.Where(p => !p.IsPrimary).GroupBy(p => p.Table).ToList();
            foreach (var groupColumn in groupColumnList)
            {
                var tableColumnList = groupColumn.ToList();
                if (tableColumnList.Count > 0)
                {
                    var firstColumn = tableColumnList.First();
                    builderColumn.AppendFormat("[{0}] = @{0}", firstColumn.Name);
                }
                for (int i = 1; i < tableColumnList.Count; i++)
                {
                    var column = tableColumnList[i];
                    builderColumn.AppendLine();
                    builderColumn.AppendFormat("  ,[{0}] = @{0}", column.Name);
                }
                builder.AppendFormat("  Update [dbo].[{0}]", groupColumn.Key);
                builder.AppendLine();
                builder.Append("  Set ");
                builder.AppendLine(builderColumn.ToString());
                builder.AppendFormat("  Where [{0}] = @{0};", primaryColumn.Name);
                builder.AppendLine();
                builder.AppendLine();
                builderColumn.Clear();
            }

            builder.AppendLine("End");
            return builder.ToString();
        }
        private static string GetProcedureTypeInsertCommand(ProcedureOperate operate, IEntitySchema view)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0} Procedure [dbo].[Insert{1}Type]", operate.ToString(), view.Name);
            builder.AppendLine();
            builder.AppendFormat("  @{0}Type [{0}Type] Readonly", view.Name);
            builder.AppendLine();
            builder.AppendLine("As");
            builder.AppendLine("Begin");
            builder.AppendLine("  Set NoCount ON;");
            builder.AppendLine();
            var columnBuilder = new StringBuilder();
            foreach (var table in view.Tables)
            {
                builder.AppendFormat("  Insert Into [dbo].[{0}]", table.Name);
                builder.AppendLine();
                builder.Append("  (");
                var columns = table.Columns.Where(p => p.Mode != ColumnMode.ReadOnly && !p.IsIdentity).ToList();
                if (columns.Count > 0)
                {
                    var column = columns.First();
                    columnBuilder.AppendFormat("[{0}]", column.Name);
                    for (int i = 1; i < columns.Count; i++)
                    {
                        column = columns[i];
                        columnBuilder.AppendLine();
                        columnBuilder.AppendFormat("  ,[{0}]", column.Name);
                    }
                }

                builder.Append(columnBuilder.ToString());
                builder.AppendLine();
                builder.Append("  ) Select ");
                builder.AppendLine(columnBuilder.ToString());
                builder.AppendFormat("  From @{0}Type;", view.Name);
                builder.AppendLine();
                builder.AppendLine();
                columnBuilder.Clear();
            }

            builder.AppendLine("End");
            return builder.ToString();
        }
        private static string GetProcedureTypeUpdateCommand(ProcedureOperate operate, IEntitySchema view)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0} Procedure [dbo].[Update{1}Type]", operate.ToString(), view.Name);
            builder.AppendLine();
            builder.AppendFormat("  @{0}Type [{0}Type] Readonly", view.Name);
            builder.AppendLine();
            builder.AppendLine("As");
            builder.AppendLine("Begin");
            builder.AppendLine("  Set NoCount ON;");
            builder.AppendLine();

            foreach (var table in view.Tables)
            {
                builder.AppendFormat("  Update [dbo].[{0}]", table.Name);
                builder.AppendLine();
                builder.Append("  Set ");
                var aryColumn = table.Columns;
                var columnList = aryColumn.Where(p => p.Mode != ColumnMode.ReadOnly).ToList();
                if (columnList.Count(p => p.IsPrimary) != 1)
                {
                    throw new ArgumentException(string.Format("table [{0}] primary error!", table.Name));
                }
                var primaryColumn = columnList.First(p => p.IsPrimary);
                columnList.Remove(primaryColumn);
                columnList.RemoveAll(p => p.IsIdentity);

                if (columnList.Count > 0)
                {
                    var firstColumn = columnList.First();
                    builder.AppendFormat("[{0}] = [ec].[{0}]", firstColumn.Name);
                }
                for (int i = 1; i < columnList.Count; i++)
                {
                    var column = columnList[i];
                    builder.AppendLine();
                    builder.AppendFormat("  ,[{0}] = [ec].[{0}]", column.Name);
                }

                builder.AppendLine();
                builder.AppendFormat("  From [dbo].[{0}]", table.Name);
                builder.AppendLine();
                builder.AppendFormat("  Inner Join @{0}Type As [ec] On [dbo].[{1}].[{2}] = [ec].[{2}];", view.Name, table.Name, primaryColumn.Name);
                builder.AppendLine();
                builder.AppendLine();
            }
            builder.AppendLine("End");
            return builder.ToString();
        }

        public static void CheckCreateDatabase(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));

            var dbConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            connectionString = connectionString.Replace(dbConnectionStringBuilder.InitialCatalog, "master");
            using (var dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                using (var dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText = string.Format("Select * From [sys].[databases] Where [name] = '{0}'", dbConnectionStringBuilder.InitialCatalog);
                    using (var reader = dbCommand.ExecuteReader())
                    {
                        if (reader.HasRows)
                            return;
                    }

                    dbCommand.CommandText = string.Format("Create Database [{0}]", dbConnectionStringBuilder.InitialCatalog);
                    dbCommand.ExecuteNonQuery();
                }
            }
        }
        private static List<DbColumn> GetDbColumnList(DbConnectionProvider connectionProvider, string name)
        {
            var resultList = new List<DbColumn>();
            var builder = new StringBuilder();
            builder.AppendLine("Select [columns].[name]");
            builder.AppendLine(",[columns].[max_length] As [MaxLength]");
            builder.AppendLine(",[types].[name] As [TypeName]");
            builder.AppendLine("From [sys].[columns]");
            builder.AppendLine("Left Join [sys].[types] On [types].[user_type_id] = [columns].[user_type_id]");
            builder.AppendFormat("Where [object_id] = Object_Id('[{0}]')", name);
            builder.AppendLine();
            builder.Append("Order By [columns].[column_id]");
            connectionProvider.ExecuteReader(builder.ToString(), CommandType.Text, null, null, reader =>
            {
                while (reader.Read())
                {
                    var column = new DbColumn();
                    column.Name = reader.GetString(0);
                    column.MaxLength = reader.GetInt16(1);
                    column.TypeName = reader.GetString(2);
                    resultList.Add(column);
                }
            });
            return resultList;
        }
        public static void CheckSchema(IEntitySchema view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var dbProvider = DbConnectionManager.Gain(view.ConnectKey, true);
            if (dbProvider.AccessLevel == AccessLevel.ReadOnly)
            {
                throw new InvalidOperationException("DbProvider key:" + view.ConnectKey + ", read only!");
            }
            var tables = view.Tables;
            if (tables.Count == 0)
            {
                throw new InvalidOperationException("no entity schema table!");
            }
            if (!tables.All(p => CheckTableChanged(p, dbProvider)))
            {
                return;
            }
            if (tables.Count >= 2)
            {
                CheckViewChanged(view, dbProvider);
            }
            if (view.Attributes.HasFlag(EntitySchemaAttributes.CreateProcedure))
            {
                CheckProcedureChanged(view, ProcedureUsage.Insert, dbProvider);
                CheckProcedureChanged(view, ProcedureUsage.Update, dbProvider);
                if (view.Attributes.HasFlag(EntitySchemaAttributes.CreateType))
                {
                    if (ProcedureExists(view, ProcedureUsage.TypeInsert, dbProvider))
                    {
                        var strComm = string.Format("Drop Procedure [dbo].[Insert{0}Type]", view.Name);
                        dbProvider.ExecuteNonQuery(strComm);
                    }
                    if (ProcedureExists(view, ProcedureUsage.TypeUpdate, dbProvider))
                    {
                        var strComm = string.Format("Drop Procedure [dbo].[Update{0}Type]", view.Name);
                        dbProvider.ExecuteNonQuery(strComm);
                    }

                    CheckTypeChanged(view, dbProvider);
                    CheckProcedureChanged(view, ProcedureUsage.TypeInsert, dbProvider);
                    CheckProcedureChanged(view, ProcedureUsage.TypeUpdate, dbProvider);
                }
            }
        }

        private static bool ViewExists(IEntitySchema view, DbConnectionProvider connectionProvider)
        {
            var strComm = string.Format("Select * From [sys].[views] Where [object_id] = Object_Id('[v_{0}]');", view.Name);
            return connectionProvider.ExecuteLines(strComm) > 0;
        }
        public static bool CheckViewChanged(IEntitySchema view, DbConnectionProvider connectionProvider)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (connectionProvider.AccessLevel == AccessLevel.ReadOnly)
                throw new ArgumentException("DbProvider key:" + view.ConnectKey + ", read only!");

            var hasChanged = false;
            if (view.Attributes.HasFlag(EntitySchemaAttributes.CreateView))
            {
                if (!ViewExists(view, connectionProvider))
                {
                    var commandText = GetViewCommand(view, ProcedureOperate.Create);
                    connectionProvider.ExecuteNonQuery(commandText);
                    hasChanged = true;
                }
                else if (view.Attributes.HasFlag(EntitySchemaAttributes.AlterView))
                {
                    var dbColumns = GetDbColumnList(connectionProvider, string.Format("v_{0}", view.Name));
                    var schemaColumns = view.Columns;
                    if (!dbColumns.All(p => schemaColumns.Any(t =>
                        p.Name.Equals(t.Name, StringComparison.CurrentCultureIgnoreCase) &&
                        p.DbType.Equals(MsSqlHelper.GetDbTypeString(t), StringComparison.CurrentCultureIgnoreCase))) ||
                        !schemaColumns.All(p => dbColumns.Any(t =>
                            p.Name.Equals(t.Name, StringComparison.CurrentCultureIgnoreCase) &&
                            MsSqlHelper.GetDbTypeString(p).Equals(t.DbType, StringComparison.CurrentCultureIgnoreCase))))
                    {
                        var commandText = GetViewCommand(view, ProcedureOperate.Alter);
                        connectionProvider.ExecuteNonQuery(commandText);
                        hasChanged = true;
                    }
                }
            }
            return hasChanged;
        }
        public static bool CheckViewChanged(IEntitySchema entitySchema)
        {
            if (entitySchema == null)
                throw new ArgumentNullException(nameof(entitySchema));

            if (!DbConnectionManager.TryGet(entitySchema.ConnectKey, out DbConnectionProvider connectionProvider))
                throw new KeyNotFoundException("not found DbProvider key:" + entitySchema.ConnectKey);
            if (connectionProvider.AccessLevel == AccessLevel.ReadOnly)
                throw new InvalidOperationException("DbProvider key:" + entitySchema.ConnectKey + ", read only!");

            return CheckViewChanged(entitySchema, connectionProvider);
        }

        private static bool TableExists(ISchemaTable table, DbConnectionProvider connectionProvider)
        {
            var str = string.Format("Select * From [sys].[tables] Where [name] = '{0}'", table.Name);
            return connectionProvider.ExecuteLines(str) > 0;
        }
        public static bool CheckTableChanged(ISchemaTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (!DbConnectionManager.TryGet(table.Schema.ConnectKey, out DbConnectionProvider connectionProvider))
                throw new KeyNotFoundException("not found DbProvider key:" + table.Schema.ConnectKey);
            if (connectionProvider.AccessLevel == AccessLevel.ReadOnly)
                throw new InvalidOperationException("DbProvider key:" + table.Schema.ConnectKey + ", read only!");

            return CheckTableChanged(table, connectionProvider);
        }
        public static bool CheckTableChanged(ISchemaTable table, DbConnectionProvider connectionProvider)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (connectionProvider.AccessLevel == AccessLevel.ReadOnly)
                throw new InvalidOperationException("DbProvider key:" + table.Schema.ConnectKey + ", read only!");

            bool hasChanged = false;
            if (table.Schema.Attributes.HasFlag(EntitySchemaAttributes.CreateTable))
            {
                if (TableExists(table, connectionProvider))
                {
                    if (table.Schema.Attributes.HasFlag(EntitySchemaAttributes.CreateColumn) ||
                        table.Schema.Attributes.HasFlag(EntitySchemaAttributes.AlterColumn) ||
                        table.Schema.Attributes.HasFlag(EntitySchemaAttributes.DropColumn))
                    {
                        var dbColumns = GetDbColumnList(connectionProvider, table.Name);
                        var columns = table.Columns;
                        if (table.Schema.Attributes.HasFlag(EntitySchemaAttributes.DropColumn))
                        {
                            foreach (var dbColumn in dbColumns)
                            {
                                if (!columns.Any(p => p.Name.Equals(dbColumn.Name, StringComparison.CurrentCultureIgnoreCase)))
                                {
                                    var commandText = string.Format("Alter Table [{0}] Drop Column [{1}]",
                                        table.Name, dbColumn.Name);
                                    connectionProvider.ExecuteNonQuery(commandText);
                                    hasChanged = true;
                                }
                            }
                        }
                        foreach (var column in columns)
                        {
                            var dbColumn = dbColumns.FirstOrDefault(p => p.Name.Equals(column.Name, StringComparison.CurrentCultureIgnoreCase));
                            if (dbColumn == null)
                            {
                                if (table.Schema.Attributes.HasFlag(EntitySchemaAttributes.CreateColumn))
                                {
                                    var commandText = string.Format("Alter Table [{0}] Add [{1}] {2}",
                                        table.Name, column.Name, MsSqlHelper.GetDbTypeString(column));
                                    connectionProvider.ExecuteNonQuery(commandText);
                                    hasChanged = true;
                                }
                            }
                            else if (!dbColumn.DbType.Equals(MsSqlHelper.GetDbTypeString(column), StringComparison.CurrentCultureIgnoreCase) && table.Schema.Attributes.HasFlag(EntitySchemaAttributes.AlterColumn))
                            {
                                var commandText = string.Format("Alter Table [{0}] Alter Column [{1}] {2}",
                                    table.Name, column.Name, MsSqlHelper.GetDbTypeString(column));
                                connectionProvider.ExecuteNonQuery(commandText);
                                hasChanged = true;
                            }
                        }
                    }
                }
                else
                {
                    var commandText = GetCreateTableCommand(table);
                    connectionProvider.ExecuteNonQuery(commandText);
                    hasChanged = true;
                }
            }
            return hasChanged;
        }

        private static bool TypeExists(string name, DbConnectionProvider connectionProvider)
        {
            var strComm = string.Format("Select * From [sys].[types] Where [name] = '{0}Type';", name);
            return connectionProvider.ExecuteLines(strComm) > 0;
        }
        public static bool CheckTypeChanged(IEntitySchema view, DbConnectionProvider connectionProvider)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (connectionProvider.AccessLevel == AccessLevel.ReadOnly)
                throw new InvalidOperationException("DbProvider key:" + view.ConnectKey + ", read only!");

            if (view.Attributes.HasFlag(EntitySchemaAttributes.CreateType))
            {
                if (TypeExists(view.Name, connectionProvider))
                {
                    var commandText = string.Format("Drop Type [dbo].[{0}Type]", view.Name);
                    connectionProvider.ExecuteNonQuery(commandText);

                    commandText = GetCreateTypeCommand(view);
                    connectionProvider.ExecuteNonQuery(commandText);
                }
                else
                {
                    var commandText = GetCreateTypeCommand(view);
                    connectionProvider.ExecuteNonQuery(commandText);
                }
            }

            return true;
        }
        public static bool CheckTypeChanged(IEntitySchema view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (!DbConnectionManager.TryGet(view.ConnectKey, out DbConnectionProvider connectionProvider))
                throw new KeyNotFoundException("not found DbProvider key:" + view.ConnectKey);
            if (connectionProvider.AccessLevel == AccessLevel.ReadOnly)
                throw new InvalidOperationException("DbProvider key:" + view.ConnectKey + ", read only!");

            return CheckTypeChanged(view, connectionProvider);
        }

        private static bool ProcedureExists(IEntitySchema view, ProcedureUsage usage, DbConnectionProvider connectionProvider)
        {
            var proceuredName = usage == ProcedureUsage.Insert || usage == ProcedureUsage.Update
                ? string.Format("{0}{1}", usage.ToString(), view.Name)
                : string.Format("{0}{1}Type", usage.ToString().TrimStart("Type".ToArray()), view.Name);
            var strComm = string.Format("Select * From sysobjects Where [xtype] = 'p' And [name] = '{0}'", proceuredName);
            return connectionProvider.ExecuteLines(strComm) > 0;
        }

        public static bool CheckProcedureChanged(IEntitySchema view, ProcedureUsage usage, DbConnectionProvider connectionProvider)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (connectionProvider.AccessLevel == AccessLevel.ReadOnly)
                throw new InvalidOperationException("DbProvider key:" + view.ConnectKey + ", read only!");

            if (ProcedureExists(view, usage, connectionProvider))
            {
                var commandText = GetProcedureCommand(view, ProcedureOperate.Alter, usage);
                connectionProvider.ExecuteNonQuery(commandText);
            }
            else
            {
                var commandText = GetProcedureCommand(view, ProcedureOperate.Create, usage);
                connectionProvider.ExecuteNonQuery(commandText);
            }
            return true;
        }

        public static bool CheckProcedureChanged(IEntitySchema view, ProcedureUsage usage)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (!DbConnectionManager.TryGet(view.ConnectKey, out DbConnectionProvider connectionProvider))
                throw new KeyNotFoundException("not found DbProvider key:" + view.ConnectKey);
            if (connectionProvider.AccessLevel == AccessLevel.ReadOnly)
                throw new InvalidOperationException("DbProvider key:" + view.ConnectKey + ", read only!");

            return CheckProcedureChanged(view, usage, connectionProvider);
        }
    }
}