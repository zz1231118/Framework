using System;
using System.Collections.Generic;
using System.Data;
using Framework.Data;

namespace Framework.Linq
{
    public static class DbConnectionProviderExtension
    {
        public static int ExecuteLines(this DbConnectionProvider connectionProvider, string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));

            int lines = 0;
            connectionProvider.ExecuteReader(commandText, commandType, parameters, reader =>
            {
                while (reader.Read())
                    lines++;
            });
            return lines;
        }
    }
}
