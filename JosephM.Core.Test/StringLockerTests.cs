#region

using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Concurrency;

#endregion

namespace JosephM.Core.Test
{
    [TestClass]
    public class StringLockerTests
    {
        private readonly List<string> _completeList = new List<string>();
        private int _testConcurrencyCountCompleted;
        private int _testThreadAbortedCountCompleted;

        [TestMethod]
        public void StringLockerTestConcurrency()
        {
            //verify the ts wait till after the first t and the u
            var stringLocker = new StringLocker();
            new Thread(() => LockXMilliseconds("t", 200, stringLocker)).Start();
            new Thread(() => LockXMilliseconds("t", 200, stringLocker)).Start();
            new Thread(() => LockXMilliseconds("t", 200, stringLocker)).Start();
            new Thread(() => LockXMilliseconds("u", 200, stringLocker)).Start();
            while (_testConcurrencyCountCompleted < 4)
                Thread.Sleep(50);

            Assert.IsTrue(_completeList[2] == "t");
            Assert.IsTrue(_completeList[3] == "t");

            Assert.Inconclusive("Index Error Thrown Some Scenario");
        }

        private void WaitXSeconds(int i)
        {
            Thread.Sleep(i);
        }

        private void LockXMilliseconds(string s, int x, StringLocker stringLocker)
        {
            stringLocker.DoWithLock(
                s,
                () =>
                {
                    WaitXSeconds(x);
                    _testConcurrencyCountCompleted++;
                    _completeList.Add(s);
                }
                );
        }

        [TestMethod]
        public void StringLockerTestThreadAbort()
        {
            //verifying the lock is released by catch when the thread aborted 
            var stringLocker = new StringLocker();
            var t = new Thread(() => LockAndWaitSecond("t", stringLocker));
            t.Start();
            Thread.Sleep(100);
            t.Abort();
            var lockObject = stringLocker.GetLockObject("t");
            lock (lockObject)
            {
                _testThreadAbortedCountCompleted++;
            }
            Assert.IsTrue(_testThreadAbortedCountCompleted == 2);
        }

        private void LockAndWaitSecond(string s, StringLocker stringLocker)
        {
            var lockObject = stringLocker.GetLockObject(s);
            lock (lockObject)
            {
                try
                {
                    Thread.Sleep(1000);
                    Assert.Fail();
                }
                catch (ThreadAbortException)
                {
                    stringLocker.ReleaseLock(s);
                    _testThreadAbortedCountCompleted++;
                }
            }
        }
    }
}