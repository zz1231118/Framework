namespace Framework.Data.MySql
{
    internal static class MySqlHelper
    {
        public const char PreParamChar = '?';
        public const char PreSymbolChar = '`';
        public const string PreParamString = "?";
        public const string PreSymbolString = "`";

        public static string FormatParamName(string paramName)
        {
            return paramName.StartsWith(PreParamString) ? paramName : PreParamChar + paramName;
        }

        public static string FormatName(string name)
        {
            return name.StartsWith("`") || name.Contains("(") ? name : $"`{name}`";
        }
    }
}
