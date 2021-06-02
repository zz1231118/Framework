using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Framework.Data;
using Framework.Data.Command;
using Framework.Data.Expressions;

namespace Framework.Linq
{
    public static class DbConnectionProviderExtension
    {
        public static int ExecuteLines(this DbConnectionProvider connectionProvider, string commandText, CommandType commandType = CommandType.Text, TimeSpan? commandTimeout = null, IEnumerable<IDataParameter>? parameters = null)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));

            int lines = 0;
            connectionProvider.ExecuteReader(commandText, commandType, commandTimeout, parameters, reader =>
            {
                while (reader.Read())
                    lines++;
            });
            return lines;
        }

        public static List<object> Select(this DbConnectionProvider connectionProvider, Type type, string commandText, CommandType commandType = CommandType.Text, TimeSpan? commandTimeout = null, IEnumerable<IDataParameter>? parameters = null)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (commandText == null)
                throw new ArgumentNullException(nameof(commandText));

            var schema = EntitySchemaManager.GetSchema(type);
            if (schema == null)
            {
                throw new InvalidConstraintException($"EntityType:{type} schema not found.");
            }
            if (schema.AccessLevel == AccessLevel.WriteOnly)
            {
                throw new ArgumentException($"Type:[{schema.EntityType}] write only");
            }
            if (schema.EntityType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new ArgumentException("not found empty type constructor!");
            }
            var result = new List<object>();
            var columns = schema.Columns.ToDictionary(p => p.Name);
            connectionProvider.ExecuteReader(commandText, commandType, commandTimeout, parameters, reader =>
            {
                object obj;
                object value;
                string name;
                while (reader.Read())
                {
                    obj = Activator.CreateInstance(type);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        value = reader[i];
                        if (value == DBNull.Value)
                        {
                            continue;
                        }
                        name = reader.GetName(i);
                        if (columns.TryGetValue(name, out ISchemaColumn column))
                        {
                            column.SetValue(obj, value);
                        }
                    }
                    result.Add(obj);
                }
            });
            return result;
        }

        public static List<T> Select<T>(this DbConnectionProvider connectionProvider, Type type, string commandText, CommandType commandType = CommandType.Text, TimeSpan? commandTimeout = null, IEnumerable<IDataParameter>? parameters = null)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (commandText == null)
                throw new ArgumentNullException(nameof(commandText));
            if (!typeof(T).IsAssignableFrom(type))
                throw new ArgumentException(nameof(type), "type as T error.");

            var schema = EntitySchemaManager.GetSchema(type);
            if (schema == null)
            {
                throw new InvalidConstraintException($"EntityType:{type} schema not found.");
            }
            if (schema.AccessLevel == AccessLevel.WriteOnly)
            {
                throw new ArgumentException($"Type:[{schema.EntityType}] write only");
            }
            if (schema.EntityType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new ArgumentException("not found empty type constructor!");
            }
            var result = new List<T>();
            var columns = schema.Columns.ToDictionary(p => p.Name);
            connectionProvider.ExecuteReader(commandText, commandType, commandTimeout, parameters, reader =>
            {
                T obj;
                object value;
                string name;
                while (reader.Read())
                {
                    obj = (T)Activator.CreateInstance(type);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        value = reader[i];
                        if (value == DBNull.Value)
                        {
                            continue;
                        }
                        name = reader.GetName(i);
                        if (columns.TryGetValue(name, out ISchemaColumn column))
                        {
                            column.SetValue(obj, value);
                        }
                    }
                    result.Add(obj);
                }
            });
            return result;
        }

        public static List<T> Select<T>(this DbConnectionProvider connectionProvider, string commandText, CommandType commandType = CommandType.Text, TimeSpan? commandTimeout = null, IEnumerable<IDataParameter>? parameters = null)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (commandText == null)
                throw new ArgumentNullException(nameof(commandText));

            var type = typeof(T);
            var schema = EntitySchemaManager.GetSchema(type);
            if (schema == null)
            {
                throw new InvalidConstraintException($"EntityType:{type} schema not found.");
            }
            if (schema.AccessLevel == AccessLevel.WriteOnly)
            {
                throw new ArgumentException($"Type:[{schema.EntityType}] write only");
            }
            if (schema.EntityType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new ArgumentException("not found empty type constructor!");
            }
            var result = new List<T>();
            var columns = schema.Columns.ToDictionary(p => p.Name);
            connectionProvider.ExecuteReader(commandText, commandType, commandTimeout, parameters, reader =>
            {
                T obj;
                string name;
                object value;
                while (reader.Read())
                {
                    obj = (T)Activator.CreateInstance(type);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        value = reader[i];
                        if (value == DBNull.Value)
                        {
                            continue;
                        }
                        name = reader.GetName(i);
                        if (columns.TryGetValue(name, out ISchemaColumn column))
                        {
                            column.SetValue(obj, value);
                        }
                    }
                    result.Add(obj);
                }
            });
            return result;
        }

        public static List<object> Select(this DbConnectionProvider connectionProvider, Type type, IDbCommandStruct commandStruct)
        {
            if (commandStruct == null)
                throw new ArgumentNullException(nameof(commandStruct));

            return Select(connectionProvider, type, commandStruct.CommandText, CommandType.Text, commandStruct.CommandTimeout, commandStruct.Parameters);
        }

        public static List<T> Select<T>(this DbConnectionProvider connectionProvider, Type type, IDbCommandStruct commandStruct)
        {
            if (commandStruct == null)
                throw new ArgumentNullException(nameof(commandStruct));

            return Select<T>(connectionProvider, type, commandStruct.CommandText, CommandType.Text, commandStruct.CommandTimeout, commandStruct.Parameters);
        }

        public static List<T> Select<T>(this DbConnectionProvider connectionProvider, IDbCommandStruct<T> commandStruct)
        {
            if (commandStruct == null)
                throw new ArgumentNullException(nameof(commandStruct));

            return Select<T>(connectionProvider, commandStruct.CommandText, CommandType.Text, commandStruct.CommandTimeout, commandStruct.Parameters);
        }

        public static List<T> Select<T>(this DbConnectionProvider connectionProvider, IDbCommandStruct commandStruct)
        {
            if (commandStruct == null)
                throw new ArgumentNullException(nameof(commandStruct));

            return Select<T>(connectionProvider, commandStruct.CommandText, CommandType.Text, commandStruct.CommandTimeout, commandStruct.Parameters);
        }

        public static void Insert(this DbConnectionProvider connectionProvider, IEnumerable<object> datas)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));

            Type type;
            using (var e = datas.GetEnumerator())
            {
                if (!e.MoveNext())
                {
                    return;
                }
                type = e.Current.GetType();
            }
            var schema = EntitySchemaManager.GetSchema(type);
            if (schema == null)
            {
                throw new InvalidConstraintException($"EntityType:{type} schema not found.");
            }
            if (schema.AccessLevel == AccessLevel.ReadOnly)
            {
                throw new ArgumentException($"Type:[{schema.EntityType}] read only");
            }
            var columns = schema.Columns;
            var validColumn = columns.Where(p => !p.IsIdentity).ToArray();
            var commandStruct = connectionProvider.CreateCommand(schema.Name, DbCommandMode.Insert, validColumn.Select(p => SqlExpression.Member(p.Name)));
            foreach (var data in datas)
            {
                foreach (var column in validColumn)
                {
                    var value = column.GetValue(data) ?? DBNull.Value;
                    commandStruct.AddParameter(column.Name, value);
                }

                connectionProvider.ExecuteNonQuery(commandStruct);
                commandStruct.ClearParameter();
            }
        }

        public static void Insert<T>(this DbConnectionProvider connectionProvider, IEnumerable<T> datas)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));

            Insert(connectionProvider, datas.Cast<object>());
        }

        public static void Update(this DbConnectionProvider connectionProvider, IEnumerable<object> datas)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));

            Type type;
            using (var e = datas.GetEnumerator())
            {
                if (!e.MoveNext())
                {
                    return;
                }
                type = e.Current.GetType();
            }
            var schema = EntitySchemaManager.GetSchema(type);
            if (schema == null)
            {
                throw new InvalidConstraintException($"EntityType:{type} schema not found.");
            }
            if (schema.AccessLevel == AccessLevel.ReadOnly)
            {
                throw new InvalidConstraintException($"Type:[{schema.EntityType}] read only");
            }
            var columns = schema.Columns;
            var keyColumn = columns.FirstOrDefault(p => p.IsPrimary);
            if (keyColumn == null)
            {
                throw new InvalidConstraintException($"Type:[{schema.EntityType}] not found parimary");
            }
            var aryColumn = columns.Where(p => !p.IsIdentity).Select(p => SqlExpression.Member(p.Name)).ToArray();
            var dbCommand = connectionProvider.CreateCommand(schema.Name, DbCommandMode.Update, aryColumn);
            foreach (var data in datas)
            {
                foreach (var column in columns)
                {
                    if (column.IsIdentity && !column.IsPrimary)
                    {
                        continue;
                    }

                    var val = column.GetValue(data) ?? DBNull.Value;
                    var parameter = connectionProvider.CreateParameter(column.Name, val);
                    dbCommand.AddParameter(parameter);
                }

                dbCommand.Condition = SqlExpression.Equal(SqlExpression.Member(keyColumn.Name), SqlExpression.Paramter(keyColumn.Name));
                var strComm = dbCommand.CommandText;
                connectionProvider.ExecuteNonQuery(strComm, parameters: dbCommand.Parameters.ToArray());
                dbCommand.ClearParameter();
            }
        }

        public static void Update<T>(this DbConnectionProvider connectionProvider, IEnumerable<T> datas)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));

            Update(connectionProvider, datas.Cast<object>());
        }

        public static void Delete(this DbConnectionProvider connectionProvider, IEnumerable<object> datas)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));

            Type type;
            using (var e = datas.GetEnumerator())
            {
                if (!e.MoveNext())
                {
                    return;
                }
                type = e.Current.GetType();
            }
            var schema = EntitySchemaManager.GetSchema(type);
            if (schema == null)
            {
                throw new InvalidConstraintException($"EntityType:{type} schema not found.");
            }
            if (schema.AccessLevel == AccessLevel.ReadOnly)
            {
                throw new ArgumentException($"Type:[{schema.EntityType}] read only");
            }
            var validColumn = schema.Columns;
            var keyColumn = validColumn.FirstOrDefault(p => p.IsPrimary);
            if (keyColumn == null)
            {
                throw new ArgumentException($"Type:[{schema.EntityType}] not found parimary");
            }
            var dbCommand = connectionProvider.CreateCommand(schema.Name, DbCommandMode.Delete);
            foreach (var data in datas)
            {
                var value = keyColumn.GetValue(data);
                if (value == null)
                {
                    throw new InvalidOperationException($"schema:{schema.EntityType} primary:{keyColumn.Name} should not be null");
                }

                var parameter = connectionProvider.CreateParameter($"@{keyColumn.Name}", value);
                dbCommand.AddParameter(parameter);
                dbCommand.Condition = SqlExpression.Equal(SqlExpression.Member(keyColumn.Name), SqlExpression.Paramter(keyColumn.Name));

                var commandText = dbCommand.CommandText;
                connectionProvider.ExecuteNonQuery(commandText, parameters: dbCommand.Parameters.ToArray());
                dbCommand.ClearParameter();
            }
        }

        public static void Delete<T>(this DbConnectionProvider connectionProvider, IEnumerable<T> datas)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));

            Delete(connectionProvider, datas.Cast<object>());
        }
    }
}
