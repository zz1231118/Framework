namespace Framework.Data
{
    public enum EntitySchemaAttributes
    {
        None = 0x00,
        CreateTable = 0x0001,
        CreateColumn = 0x0002,
        CreateView = 0x0004,
        CreateProcedure = 0x0008,
        CreateType = 0x0010,

        AlterTable = 0x0020,
        AlterColumn = 0x0030,
        AlterView = 0x0080,
        AlterProcedure = 0x0100,

        DropColumn = 0x0400,

        /// <summary>
        /// 保守主义
        /// </summary>
        Conservatism = CreateColumn | CreateTable | CreateView | CreateProcedure | CreateType,
        /// <summary>
        /// 自由主义
        /// </summary>
        Liberalism = CreateColumn | CreateTable | CreateView | CreateProcedure | CreateType |
            AlterColumn | AlterTable | AlterView | AlterProcedure,
        /// <summary>
        /// 激进主义
        /// </summary>
        Radicalism = CreateColumn | CreateTable | CreateView | CreateProcedure | CreateType |
            AlterColumn | AlterTable | AlterView | AlterProcedure |
            DropColumn,
    }
}
