using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentList.Tests
{
    [TestClass]
    public class ConcurrentListTests
    {
        private readonly int _startCount = 500_000;
        private ConcurrentList<int> _list;

        [TestInitialize]
        public void Setup()
        {
            _list = new ConcurrentList<int>();
            for (var i = 0; i < _startCount; i++)
            {
                _list.Add(i);
            }
        }


        [TestMethod]
        public void IListTests()
        {
            Assert.IsTrue(_list.Contains(500));
            Assert.IsTrue(_list.Contains(100_002));

            var reset1 = new ManualResetEvent(false);
            var reset2 = new ManualResetEvent(false);

            // Add and remove items from two separate background threads.
            _ = Task.Run(() =>
            {
                for (var i = 0; i < 5_000; i++)
                {
                    Assert.IsTrue(_list.Remove(500 + i));
                    _list.RemoveAt(100_000);
                    _list.Add(42);
                    _list.Insert(200_000, 100);
                }
                reset1.Set();
            });

            _ = Task.Run(() =>
            {
                for (var i = 5_000; i < 10_000; i++)
                {
                    Assert.IsTrue(_list.Remove(500 + i));
                    _list.RemoveAt(100_000);
                    _list.Add(42);
                    _list.Insert(200_000, 100);
                }
                reset2.Set();
            });

            reset1.WaitOne();
            reset2.WaitOne();

            Assert.IsFalse(_list.Contains(500));
            Assert.IsFalse(_list.Contains(100_002));
            // We should have the original count with which we started.
            Assert.AreEqual(_startCount, _list.Count);
        }

        [TestMethod]
        public void ForAsync_GivenMemberReentrancy_Throws()
        {
            var result = _list.ForAsync(i =>
            {
                _list.Add(3);
            }).GetAwaiter().GetResult();

            Assert.IsTrue(result.First() is InvalidOperationException);
        }

        [TestMethod]
        public void ForEachAsync_GivenMemberReentrancy_Throws()
        {
            var result = _list.ForEachAsync(item =>
            {
                _list.Remove(item);
            }).GetAwaiter().GetResult();

            Assert.IsTrue(result.First() is InvalidOperationException);
        }


        [TestMethod]
        public void ForAsync_GivenCollectionModifiedInAnotherThread_WaitsAndSucceeds()
        {
            var reset = new ManualResetEvent(false);
            var loopStart = new ManualResetEvent(false);
            var lastItem = _list.Last();
            var lastItemFound = false;

            _ = Task.Run(async () =>
            {
                await _list.ForAsync(i =>
                {
                    loopStart.Set();
                    if (i == lastItem)
                    {
                        lastItemFound = true;
                    }
                });
                reset.Set();
            });

            loopStart.WaitOne();

            Assert.IsTrue(_list.Remove(lastItem));

            // The last item was found, so we know the remove operation
            // waited until the loop was finished.
            Assert.IsTrue(lastItemFound);

            reset.WaitOne();
        }

        [TestMethod]
        public void ForEachAsync_GivenCollectionModifiedInAnotherThread_WaitsAndSucceeds()
        {
            var reset = new ManualResetEvent(false);
            var loopStart = new ManualResetEvent(false);
            var lastItem = _list.Last();
            var lastItemFound = false;

            _ = Task.Run(async () =>
            {
                await _list.ForEachAsync(i =>
                {
                    loopStart.Set();
                    if (i == lastItem)
                    {
                        lastItemFound = true;
                    }
                });
                reset.Set();
            });

            loopStart.WaitOne();
            
            Assert.IsTrue(_list.Remove(lastItem));

            // The last item was found, so we know the remove operation
            // waited until the loop was finished.
            Assert.IsTrue(lastItemFound);

            reset.WaitOne();
        }

        [TestMethod]
        public void QueueAction_GivenNotInLoop_Throws()
        {
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                _list.QueueAction(() => Console.WriteLine("Test"));
            });
        }

        [TestMethod]
        public void QueueAction_GivenFromWithinForEachLoop_ExecutesAction()
        {
            var count1 = 0;
            var count2 = 0;

            var exceptions = _list.ForEachAsync(item =>
            {
                count1++;

                _list.QueueAction(() =>
                {
                    count2++;
                });
            }).GetAwaiter().GetResult();

            Assert.AreEqual(0, exceptions.Count);
            Assert.AreEqual(500_000, count1);
            Assert.AreEqual(500_000, count2);
        }

        [TestMethod]
        public void QueueAction_GivenFromWithinForLoop_ExecutesAction()
        {
            var count1 = 0;
            var count2 = 0;

            var exceptions = _list.ForAsync(item =>
            {
                count1++;

                _list.QueueAction(() =>
                {
                    count2++;
                });
            }).GetAwaiter().GetResult();

            Assert.AreEqual(0, exceptions.Count);
            Assert.AreEqual(500_000, count1);
            Assert.AreEqual(500_000, count2);
        }
    }
}
