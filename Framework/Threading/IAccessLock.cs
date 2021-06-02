using System;

namespace Framework.Threading
{
    public interface IAccessLock : IDisposable
    {
        bool IsLocked { get; }

        AccessLockType Type { get; }
    }

    public interface IAccessLock<T> : IAccessLock
    {
        T Value { get; }
    }
}
