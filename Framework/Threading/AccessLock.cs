using System;
using System.Threading;

namespace Framework.Threading
{
    public abstract class AccessLock : BaseDisposed
    {
        private ReaderWriterLockSlim rwLockSlim = new ReaderWriterLockSlim();

        protected bool Enter(IAccessLock rwlock)
        {
            CheckDisposed();
            switch (rwlock.Type)
            {
                case AccessLockType.Reader:
                    if (!rwLockSlim.IsReadLockHeld && !rwLockSlim.IsUpgradeableReadLockHeld && !rwLockSlim.IsWriteLockHeld)
                    {
                        rwLockSlim.EnterReadLock();
                        return true;
                    }
                    break;
                case AccessLockType.UpgradeableReader:
                    if (rwLockSlim.IsReadLockHeld)
                        throw new InvalidOperationException("already holds a read lock!");

                    if (!rwLockSlim.IsUpgradeableReadLockHeld && !rwLockSlim.IsWriteLockHeld)
                    {
                        rwLockSlim.EnterUpgradeableReadLock();
                        return true;
                    }
                    break;
                case AccessLockType.Writer:
                    if (rwLockSlim.IsReadLockHeld)
                        throw new InvalidOperationException("already holds a read lock!");

                    if (!rwLockSlim.IsWriteLockHeld)
                    {
                        rwLockSlim.EnterWriteLock();
                        return true;
                    }
                    break;
                default:
                    throw new NotSupportedException(rwlock.Type.ToString());
            }

            return false;
        }

        protected void Exit(IAccessLock rwlock)
        {
            if (rwlock.IsLocked)
            {
                switch (rwlock.Type)
                {
                    case AccessLockType.Reader:
                        rwLockSlim.ExitReadLock();
                        break;
                    case AccessLockType.UpgradeableReader:
                        rwLockSlim.ExitUpgradeableReadLock();
                        break;
                    case AccessLockType.Writer:
                        rwLockSlim.ExitWriteLock();
                        break;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                try
                {
                    if (rwLockSlim.IsReadLockHeld)
                        rwLockSlim.ExitReadLock();
                    if (rwLockSlim.IsWriteLockHeld)
                        rwLockSlim.ExitWriteLock();
                    if (rwLockSlim.IsUpgradeableReadLockHeld)
                        rwLockSlim.ExitUpgradeableReadLock();

                    rwLockSlim.Dispose();
                    rwLockSlim = null;
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }
    }
}
