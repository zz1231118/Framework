using System;
using System.Threading;

namespace Framework.Threading
{
    public class AccessLockSlim : AccessLock
    {
        public IAccessLock EnterReaderLock()
        {
            return new AccessLockInternel(this, AccessLockType.Reader);
        }

        public IAccessLock EnterUpgradeableReadLock()
        {
            return new AccessLockInternel(this, AccessLockType.UpgradeableReader);
        }

        public IAccessLock EnterWriterLock()
        {
            return new AccessLockInternel(this, AccessLockType.Writer);
        }

        struct AccessLockInternel : IAccessLock
        {
            private readonly AccessLockType type;
            private readonly bool locked;
            private AccessLockSlim? accessLockSlim;

            public AccessLockInternel(AccessLockSlim accessLock, AccessLockType lockType)
            {
                accessLockSlim = accessLock;
                type = lockType;
                locked = false;
                locked = accessLock.Enter(this);
            }

            public bool IsLocked => locked;

            public AccessLockType Type => type;

            public void Dispose()
            {
                var accessLock = Interlocked.Exchange(ref accessLockSlim, null);
                if (accessLock != null && locked)
                {
                    accessLock.Exit(this);
                }
            }
        }
    }

    public class AccessLockSlim<T> : AccessLock
    {
        private readonly T value;

        public AccessLockSlim(T value)
        {
            this.value = value;
        }

        public T Value => value;

        public IAccessLock<T> EnterReaderLock()
        {
            return new AccessLockInternel(this, AccessLockType.Reader);
        }
        public IAccessLock<T> EnterUpgradeableReadLock()
        {
            return new AccessLockInternel(this, AccessLockType.UpgradeableReader);
        }
        public IAccessLock<T> EnterWriterLock()
        {
            return new AccessLockInternel(this, AccessLockType.Writer);
        }

        struct AccessLockInternel : IAccessLock<T>
        {
            private readonly AccessLockType type;
            private readonly bool locked;
            private AccessLockSlim<T>? accessLockSlim;

            public AccessLockInternel(AccessLockSlim<T> accessLock, AccessLockType lockType)
            {
                accessLockSlim = accessLock;
                type = lockType;
                locked = false;
                locked = accessLock.Enter(this);
            }

            public bool IsLocked => locked;

            public T Value
            {
                get 
                {
                    if (accessLockSlim == null)
                    {
                        throw new ObjectDisposedException(GetType().FullName);
                    }

                    return accessLockSlim.Value;
                }
            }

            public AccessLockType Type => type;

            public void Dispose()
            {
                var accessLock = Interlocked.Exchange(ref accessLockSlim, null);
                if (accessLock != null && locked)
                {
                    accessLock.Exit(this);
                }
            }
        }
    }
}
