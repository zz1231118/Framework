using System;
using System.Collections.Generic;
using System.Data;

namespace Framework.Data.MsSql
{
    public static class MsSqlHelper
    {
        public const char PreParamChar = '@';
        public const string PreParamString = "@";

        private static readonly Dictionary<Type, SqlDbType> typeToDbTypeMappings = new Dictionary<Type, SqlDbType>()
        {
            [typeof(bool)] = SqlDbType.Bit,
            [typeof(byte)] = SqlDbType.TinyInt,
            [typeof(sbyte)] = SqlDbType.TinyInt,
            [typeof(short)] = SqlDbType.SmallInt,
            [typeof(ushort)] = SqlDbType.SmallInt,
            [typeof(int)] = SqlDbType.Int,
            [typeof(uint)] = SqlDbType.Int,
            [typeof(long)] = SqlDbType.BigInt,
            [typeof(ulong)] = SqlDbType.BigInt,
            [typeof(float)] = SqlDbType.Real,
            [typeof(double)] = SqlDbType.Float,
            [typeof(decimal)] = SqlDbType.Decimal,
            [typeof(string)] = SqlDbType.NVarChar,
            [typeof(DateTime)] = SqlDbType.DateTime,
            [typeof(Guid)] = SqlDbType.UniqueIdentifier,
            [typeof(byte[])] = SqlDbType.Binary,
        };

        public static string FormatParamName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return name.StartsWith(PreParamString) ? name : PreParamString + name;
        }

        public static string FormatSymbolName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return name.StartsWith("[") || name.Contains("(") || name.Contains(PreParamString) ? name : string.Format("[{0}]", name);
        }

        public static SqlDbType ConvertToSqlType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!typeToDbTypeMappings.TryGetValue(type, out SqlDbType dbType))
            {
                throw new Exception($"[{type.Name}] can't change to SqlDbType");
            }

            return dbType;
        }

        public static SqlDbType ConvertToSqlType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString: return SqlDbType.VarChar;
                case DbType.Binary: return SqlDbType.Binary;
                case DbType.Byte: return SqlDbType.TinyInt;
                case DbType.Boolean: return SqlDbType.Bit;
                case DbType.Currency: return SqlDbType.Money;
                case DbType.Date: return SqlDbType.Date;
                case DbType.DateTime: return SqlDbType.DateTime;
                case DbType.Decimal: return SqlDbType.Decimal;
                case DbType.Double: return SqlDbType.Float;
                case DbType.Guid: return SqlDbType.UniqueIdentifier;
                case DbType.Int16: return SqlDbType.SmallInt;
                case DbType.Int32: return SqlDbType.Int;
                case DbType.Int64: return SqlDbType.BigInt;
                case DbType.Object: return SqlDbType.Binary;
                case DbType.SByte: return SqlDbType.TinyInt;
                case DbType.Single: return SqlDbType.Real;
                case DbType.String: return SqlDbType.NVarChar;
                case DbType.Time: return SqlDbType.Time;
                case DbType.UInt16: return SqlDbType.SmallInt;
                case DbType.UInt32: return SqlDbType.Int;
                case DbType.UInt64: return SqlDbType.BigInt;
                case DbType.VarNumeric: return SqlDbType.Decimal;
                case DbType.AnsiStringFixedLength: return SqlDbType.VarChar;
                case DbType.StringFixedLength: return SqlDbType.NVarChar;
                case DbType.Xml: return SqlDbType.Xml;
                case DbType.DateTime2: return SqlDbType.DateTime2;
                case DbType.DateTimeOffset: return SqlDbType.DateTimeOffset;
                default: throw new ArgumentException(string.Format("unknown DbType:{0}", dbType));
            }
        }

        public static string GetDbTypeString(ISchemaColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            switch (column.DbType)
            {
                case DbType.Byte:
                case DbType.Boolean:
                case DbType.Date:
                case DbType.DateTime:
                case DbType.Double:
                case DbType.Guid:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.SByte:
                case DbType.Single:
                case DbType.Time:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.Xml:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return string.Format("{0}", ConvertToSqlType(column.DbType));
                case DbType.AnsiString:
                case DbType.Binary:
                case DbType.Currency:
                case DbType.Decimal:
                case DbType.String:
                case DbType.VarNumeric:
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                    return string.Format("{0}({1})", ConvertToSqlType(column.DbType), column.MaxLength == -1 ? "Max" : column.MaxLength.ToString());
                default:
                    throw new ArgumentException(string.Format("unknown DbType:{0}", column.DbType));
            }
        }
    }
}
