namespace Framework.Data
{
    class DbColumn
    {
        public string Name { get; set; }

        public string TypeName { get; set; }

        public short MaxLength { get; set; }

        public string DbType
        {
            get
            {
                switch (TypeName.ToLower())
                {
                    case "varchar":
                        return string.Format("VarChar({0})",
                            MaxLength == -1
                            ? "Max"
                            : MaxLength.ToString());
                    case "nvarchar":
                        return string.Format("NVarChar({0})",
                            MaxLength == -1
                            ? "Max"
                            : (MaxLength / 2).ToString());
                    default:
                        return TypeName;
                }
            }
        }
    }
}