namespace Framework.Data
{
    public class DbCommandJoin
    {
        private readonly DbCommandJoinMode _mode;
        private readonly string _tableName;
        private string _condition;

        internal DbCommandJoin(DbCommandJoinMode mode, string tableName, string condition = "")
        {
            _mode = mode;
            _tableName = tableName;
            _condition = condition;
        }

        public DbCommandJoinMode Mode
        {
            get { return _mode; }
        }
        public string TableName
        {
            get { return _tableName; }
        }
        public string Condition
        {
            get { return _condition; }
            set { _condition = value; }
        }
        public string Sql
        {
            get;
            private set;
        }

        internal void Parser()
        {
            Sql = string.Format("{0} Join [{1}] On {2}", _mode.ToString(), _tableName, _condition);
        }
    }

    public enum DbCommandJoinMode : byte
    {
        Inner,
        Left,
        Right,
    }
}