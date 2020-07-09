namespace Framework.Data.Entry
{
    public enum EntityState : byte
    {
        Detached = 0x01,
        Unchanged = 0x02,
        Added = 0x04,
        Deleted = 0x08,
        Modified = 0x10
    }
}
