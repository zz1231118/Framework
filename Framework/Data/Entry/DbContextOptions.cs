namespace Framework.Data.Entry
{
    public class DbContextOptions
    {
        public static readonly DbContextOptions Default = new DbContextOptions()
        {
            RowSeparatorCount = 10000,
        };

        public int RowSeparatorCount { get; set; }
    }
}
