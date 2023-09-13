using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bitbound.ConcurrentList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bitbound.ConcurrentList.Tests
{
    [TestClass]
    public class ConcurrentListTests
    {
        private ConcurrentList<int> _list;

        [TestInitialize]
        public void Setup()
        {
            _list = new ConcurrentList<int>();
        }


        [TestMethod]
        public void MultipleAddOperations_GivenMultipleThreads_Ok()
        {
            var reset1 = new ManualResetEvent(false);
            var reset2 = new ManualResetEvent(false);
            var exceptions = 0;
            var count = 10_000;

            // Add items from two separate background threads.
            _ = Task.Run(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    try
                    {
                        if (i % 2 != 0)
                        {
                            continue;
                        }

                        _list.Add(i);
                    }
                    catch
                    {
                        Interlocked.Increment(ref exceptions);
                    }
                }
                reset1.Set();
            });

            _ = Task.Run(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    try 
                    {
                        if (i % 2 == 0)
                        {
                            continue;
                        }

                        _list.Add(i);
                    }
                    catch
                    {
                        Interlocked.Increment(ref exceptions);
                    }
                }
                reset2.Set();
            });

            reset1.WaitOne();
            reset2.WaitOne();

            Assert.AreEqual(0, exceptions);
            Assert.AreEqual(count, _list.Count);

            for (var i = 0; i < count; i++)
            {
                Assert.IsTrue(_list.Contains(i));
            }
        }

        [TestMethod]
        public void MultipleRemoveOperations_GivenMultipleThreads_Ok()
        {
            var reset1 = new ManualResetEvent(false);
            var reset2 = new ManualResetEvent(false);
            var exceptions = 0;
            var count = 10_000;

            _list.AddRange(Enumerable.Range(0, count));

            // Add items from two separate background threads.
            _ = Task.Run(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    try
                    {
                        if (i % 2 != 0)
                        {
                            continue;
                        }

                        _list.Remove(i);
                    }
                    catch
                    {
                        Interlocked.Increment(ref exceptions);
                    }
                }
                reset1.Set();
            });

            _ = Task.Run(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    try
                    {
                        if (i % 2 == 0)
                        {
                            continue;
                        }

                        _list.Remove(i);
                    }
                    catch
                    {
                        Interlocked.Increment(ref exceptions);
                    }
                }
                reset2.Set();
            });

            reset1.WaitOne();
            reset2.WaitOne();

            Assert.AreEqual(0, exceptions);
            Assert.AreEqual(0, _list.Count);
        }
    }
}
