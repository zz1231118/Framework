using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Framework.Data
{
    public interface IDbConnection : IDisposable
    {
        DbConnectionCategory Category { get; }
        ConnectionState State { get; }
        DbConnection DbConnection { get; }
        bool IsTemporary { get; }

        void Open();
        void Close();
        void CheckConnect();

        int ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null);
        IDataReader ExecuteReader(string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null);
        object ExecuteScalar(string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null);
        T ExecuteScalar<T>(string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null);
    }
}
