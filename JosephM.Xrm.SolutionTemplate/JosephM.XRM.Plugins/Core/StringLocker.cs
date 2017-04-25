#region

using System;
using System.Collections.Generic;

#endregion

namespace $safeprojectname$.Core
{
    /// <summary>
    ///     Implements locking on a string - call GetLockObject then ReleaseLock after the lock code
    /// </summary>
    public class StringLocker
    {
        private readonly SortedDictionary<string, IdLock> _idLocks = new SortedDictionary<string, IdLock>();
        private readonly object _lockObject = new object();

        public void DoWithLock(string lockString, Action action)
        {
            var lockObject = GetLockObject(lockString);
            lock (lockObject)
            {
                try
                {
                    action();
                }
                finally
                {
                    ReleaseLock(lockString);
                }
            }
        }

        /// <summary>
        ///     Retrieves a lock object for the string and increments the lock count
        /// </summary>
        internal object GetLockObject(string keyValue)
        {
            lock (_lockObject)
            {
                IdLock thisLock;
                if (_idLocks.ContainsKey(keyValue))
                    thisLock = _idLocks[keyValue];
                else
                {
                    thisLock = new IdLock();
                    _idLocks.Add(keyValue, thisLock);
                }
                thisLock.NumberOfConnections++;
                return thisLock.LockObject;
            }
        }

        /// <summary>
        ///     Decrements the lock count for this string
        /// </summary>
        internal void ReleaseLock(string keyValue)
        {
            lock (_lockObject)
            {
                if (_idLocks.ContainsKey(keyValue))
                {
                    _idLocks[keyValue].NumberOfConnections--;
                    if (_idLocks[keyValue].NumberOfConnections == 0)
                        _idLocks.Remove(keyValue);
                }
            }
        }

        /// <summary>
        ///     Store detail for a specific string being locked
        /// </summary>
        private class IdLock
        {
            private readonly object _lockObject = new Object();

            public IdLock()
            {
                NumberOfConnections = 0;
            }

            internal int NumberOfConnections { get; set; }

            internal object LockObject
            {
                get { return _lockObject; }
            }
        }
    }
}