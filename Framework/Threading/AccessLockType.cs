namespace Framework.Threading
{
    public enum AccessLockType : byte
    {
        Reader,
        UpgradeableReader,
        Writer,
    }
}
