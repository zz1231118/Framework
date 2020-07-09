using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Framework.Data.Command;
using Framework.Data.Expressions;

namespace Framework.Data
{
    public static class DbHelper
    {
        public static List<object> Load(Type type, DbConnectionProvider connectionProvider, string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (commandText == null)
                throw new ArgumentNullException(nameof(commandText));
            if (type.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException("not found empty type constructor!");

            var entitySchema = EntitySchemaManager.GetSchema(type);
            if (entitySchema == null)
                throw new InvalidConstraintException(string.Format("EntityType:{0} schema not found.", type));
            if (entitySchema.AccessLevel == AccessLevel.WriteOnly)
                throw new ArgumentException(string.Format("Type:[{0}] write only", type));

            var result = new List<object>();
            var columns = entitySchema.Columns.ToDictionary(p => p.Name);
            connectionProvider.ExecuteReader(commandText, commandType, parameters, reader =>
            {
                ISchemaColumn column;
                while (reader.Read())
                {
                    var obj = Activator.CreateInstance(type);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader[i];
                        if (value == DBNull.Value)
                            continue;

                        var name = reader.GetName(i);
                        if (columns.TryGetValue(name, out column))
                            column.SetValue(obj, value);
                    }
                    result.Add(obj);
                }
            });
            return result;
        }

        public static List<T> Load<T>(DbConnectionProvider connectionProvider, string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null)
        {
            return Load(typeof(T), connectionProvider, commandText, commandType, parameters).Cast<T>().ToList();
        }

        public static List<T> Load<T>(string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null)
        {
            var entitySchema = EntitySchemaManager.GetSchema<T>();
            if (entitySchema == null)
                throw new InvalidConstraintException(string.Format("EntityType:{0} schema not found.", typeof(T)));
            if (!DbConnectionManager.TryGet(entitySchema.ConnectKey, out DbConnectionProvider connectionProvider))
                throw new InvalidOperationException("not found DbProvider key:" + entitySchema.ConnectKey);

            return Load<T>(connectionProvider, commandText, commandType, parameters);
        }

        public static List<T> Load<T>(DbConnectionProvider connectionProvider, IDbCommandStruct commandStruct)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (commandStruct == null)
                throw new ArgumentNullException(nameof(commandStruct));

            return Load<T>(connectionProvider, commandStruct.CommandText, parameters: commandStruct.Parameters.ToArray());
        }

        public static List<T> Load<T>(IDbCommandStruct commandStruct)
        {
            if (commandStruct == null)
                throw new ArgumentNullException(nameof(commandStruct));

            return Load<T>(commandStruct.CommandText, parameters: commandStruct.Parameters.ToArray());
        }

        public static void Insert(DbConnectionProvider connectionProvider, IEnumerable<object> datas)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));
            if (datas.Count() == 0)
                throw new ArgumentException("data count is zero");

            var type = datas.First().GetType();
            if (datas.Any(p => p.GetType() != type))
                throw new ArgumentException("different data types");
            var entitySchema = EntitySchemaManager.GetSchema(type);
            if (entitySchema == null)
                throw new InvalidConstraintException(string.Format("EntityType:{0} schema not found.", type));
            if (entitySchema.AccessLevel == AccessLevel.ReadOnly)
                throw new ArgumentException(string.Format("Type:[{0}] read only", type));

            var columns = entitySchema.Columns;
            var validColumn = columns.Where(p => !p.IsIdentity).ToArray();
            var commandStruct = connectionProvider.CreateCommand(entitySchema.Name, DbCommandMode.Insert, validColumn.Select(p => SqlExpression.Member(p.Name)));
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

        public static void Insert<T>(IEnumerable<T> datas)
        {
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));

            var entitySchema = EntitySchemaManager.GetSchema<T>();
            if (entitySchema == null)
                throw new InvalidConstraintException(string.Format("EntityType:{0} schema not found.", typeof(T)));
            if (!DbConnectionManager.TryGet(entitySchema.ConnectKey, out DbConnectionProvider connectionProvider))
                throw new InvalidOperationException("not found DbProvider key:" + entitySchema.ConnectKey);

            Insert(connectionProvider, datas.Cast<object>());
        }

        public static void Update(DbConnectionProvider connectionProvider, IEnumerable<object> datas)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));
            if (datas.Count() == 0)
                throw new ArgumentException("data count is zero");

            var type = datas.First().GetType();
            if (datas.All(p => p.GetType() != type))
                throw new ArgumentException("Different data types");
            var entitySchema = EntitySchemaManager.GetSchema(type);
            if (entitySchema == null)
                throw new InvalidConstraintException(string.Format("EntityType:{0} schema not found.", type));
            if (entitySchema.AccessLevel == AccessLevel.ReadOnly)
                throw new ArgumentException(string.Format("Type:[{0}] read only", type));

            var columns = entitySchema.Columns;
            var keyColumn = columns.FirstOrDefault(p => p.IsPrimary);
            if (keyColumn == null)
                throw new ArgumentException(string.Format("Type:[{0}] not found parimary", type));

            var aryColumn = columns.Where(p => !p.IsIdentity).Select(p => SqlExpression.Member(p.Name)).ToArray();
            var dbCommand = connectionProvider.CreateCommand(entitySchema.Name, DbCommandMode.Update, aryColumn);
            foreach (var data in datas)
            {
                foreach (var column in columns)
                {
                    if (column.IsIdentity && !column.IsPrimary)
                        continue;

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

        public static void Update<T>(IEnumerable<T> datas)
        {
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));

            var entitySchema = EntitySchemaManager.GetSchema<T>();
            if (entitySchema == null)
                throw new InvalidConstraintException(string.Format("EntityType:{0} schema not found.", typeof(T)));
            if (!DbConnectionManager.TryGet(entitySchema.ConnectKey, out DbConnectionProvider connectionProvider))
                throw new InvalidOperationException("not found DbProvider key:" + entitySchema.ConnectKey);

            Update(connectionProvider, datas.Cast<object>());
        }

        public static void Delete(DbConnectionProvider connectionProvider, IEnumerable<object> datas)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));
            if (datas.Count() == 0)
                throw new ArgumentException("data count is zero");

            var type = datas.First().GetType();
            if (datas.All(p => p.GetType() != type))
                throw new ArgumentException("Different data types");
            var entitySchema = EntitySchemaManager.GetSchema(type);
            if (entitySchema == null)
                throw new InvalidConstraintException(string.Format("EntityType:{0} schema not found.", type));
            if (entitySchema.AccessLevel == AccessLevel.ReadOnly)
                throw new ArgumentException(string.Format("Type:[{0}] read only", type));

            var validColumn = entitySchema.Columns;
            var keyColumn = validColumn.FirstOrDefault(p => p.IsPrimary);
            if (keyColumn == null)
                throw new ArgumentException(string.Format("Type:[{0}] not found parimary", type));

            var dbCommand = connectionProvider.CreateCommand(entitySchema.Name, DbCommandMode.Delete);
            foreach (var data in datas)
            {
                var keyValue = keyColumn.GetValue(data);
                var keyParameter = connectionProvider.CreateParameter(string.Format("@{0}", keyColumn.Name), keyValue);
                dbCommand.AddParameter(keyParameter);
                dbCommand.Condition = SqlExpression.Equal(SqlExpression.Member(keyColumn.Name), SqlExpression.Paramter(keyColumn.Name));
                var strComm = dbCommand.CommandText;
                connectionProvider.ExecuteNonQuery(strComm, parameters: dbCommand.Parameters.ToArray());
                dbCommand.ClearParameter();
            }
        }

        public static void Delete<T>(IEnumerable<T> datas)
        {
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));

            var entitySchema = EntitySchemaManager.GetSchema<T>();
            if (entitySchema == null)
                throw new InvalidConstraintException(string.Format("EntityType:{0} schema not found.", typeof(T)));
            if (!DbConnectionManager.TryGet(entitySchema.ConnectKey, out DbConnectionProvider connectionProvider))
                throw new InvalidOperationException("not found DbProvider key:" + entitySchema.ConnectKey);

            Delete(connectionProvider, datas.Cast<object>());
        }
    }
}