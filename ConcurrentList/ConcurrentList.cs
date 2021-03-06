using ConcurrentList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Concurrent
{
    /// <summary>
    /// A simple, lock-based implementation of a thread-safe List<T>.
    /// Note that it will still throw if the collection is modified during
    /// enumeration.  Instead, use the ForAsync and ForEach async methods,
    /// which are thread-safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentList<T> : IList<T>
    {
        private readonly ConcurrentDictionary<int, string> _loopThreads = new ConcurrentDictionary<int, string>();
        private readonly List<T> _list = new List<T>();
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public int Count
        {
            get
            {
                CheckLoopThreads();
                try
                {
                    _lock.Wait();
                    return _list.Count;
                }
                finally
                {
                    _lock.Release();
                }
            }
        }

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get
            {
                CheckLoopThreads();
                try
                {
                    _lock.Wait();
                    return _list[index];
                }
                finally
                {
                    _lock.Release();
                }
            }
            set
            {
                CheckLoopThreads();
                try
                {
                    _lock.Wait();
                    _list[index] = value;
                }
                finally
                {
                    _lock.Release();
                }
            }
        }
        public void Add(T item)
        {
            CheckLoopThreads();
            try
            {
                _lock.Wait();
                _list.Add(item);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            CheckLoopThreads();
            try
            {
                _lock.Wait();
                _list.AddRange(items);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Clear()
        {
            CheckLoopThreads();
            try
            {
                _lock.Wait();
                _list.Clear();
            }
            finally
            {
                _lock.Release();
            }
        }

        public bool Contains(T item)
        {
            CheckLoopThreads();
            try
            {
                _lock.Wait();
                return _list.Contains(item);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            CheckLoopThreads();
            try
            {
                _lock.Wait();
                _list.CopyTo(array, arrayIndex);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Perform an action for each index in the List.  You cannot access other members of
        /// this ConcurrentList from within the supplied action.
        /// </summary>
        /// <param name="iterateAction">An Action that takes the current index as a parameter.</param>
        /// <returns>Awaitable task.</returns>
        public async Task ForAsync(Action<int> action)
        {
            CheckLoopThreads();
            try
            {
                await _lock.WaitAsync();

                _loopThreads.TryAdd(Thread.CurrentThread.ManagedThreadId, string.Empty);

                for (var i = 0; i < _list.Count; i++)
                {
                    action.Invoke(i);
                }
            }
            finally
            {
                _loopThreads.TryRemove(Thread.CurrentThread.ManagedThreadId, out _);
                _lock.Release();
            }
        }

        /// <summary>
        /// Perform an action for each item in the List.  You cannot access other members of
        /// this ConcurrentList from within the supplied action.
        /// </summary>
        /// <param name="action">An action that takes the current List item as a parameter.</param>
        /// <returns></returns>
        public async Task ForEachAsync(Action<T> action)
        {
            CheckLoopThreads();
            try
            {
                await _lock.WaitAsync();

                _loopThreads.TryAdd(Thread.CurrentThread.ManagedThreadId, string.Empty);

                foreach (var item in _list)
                {
                    action.Invoke(item);
                }
            }
            finally
            {
                _loopThreads.TryRemove(Thread.CurrentThread.ManagedThreadId, out _);
                _lock.Release();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            CheckLoopThreads();

            _lock.Wait();

            _loopThreads.TryAdd(Thread.CurrentThread.ManagedThreadId, string.Empty);

            var enumerator = new ObservableEnumerator<T>(_list);
            enumerator.Disposed += (sender, args) =>
            {
                _loopThreads.TryRemove(Thread.CurrentThread.ManagedThreadId, out _);
                _lock.Release();
            };

            return enumerator;
        }

       

        IEnumerator IEnumerable.GetEnumerator()
        {
            CheckLoopThreads();

            _lock.Wait();

            _loopThreads.TryAdd(Thread.CurrentThread.ManagedThreadId, string.Empty);

            var enumerator = new ObservableEnumerator<T>(_list);
            enumerator.Disposed += (sender, args) =>
            {
                _loopThreads.TryRemove(Thread.CurrentThread.ManagedThreadId, out _);
                _lock.Release();
            };

            return enumerator;
        }

        public int IndexOf(T item)
        {
            CheckLoopThreads();
            try
            {
                _lock.Wait();
                return _list.IndexOf(item);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Insert(int index, T item)
        {
            CheckLoopThreads();
            try
            {
                _lock.Wait();
                _list.Insert(index, item);
            }
            finally
            {
                _lock.Release();
            }
        }

        public bool Remove(T item)
        {
            CheckLoopThreads();
            try
            {
                _lock.Wait();
                return _list.Remove(item);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void RemoveAll(Predicate<T> predicate)
        {
            CheckLoopThreads();
            try
            {
                _lock.Wait();
                _list.RemoveAll(predicate);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void RemoveAt(int index)
        {
            CheckLoopThreads();
            try
            {
                _lock.Wait();
                _list.RemoveAt(index);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void RemoveRange(int index, int count)
        {
            CheckLoopThreads();
            try
            {
                _lock.Wait();
                _list.RemoveRange(index, count);
            }
            finally
            {
                _lock.Release();
            }
        }

        private void CheckLoopThreads()
        {
            if (_loopThreads.ContainsKey(Thread.CurrentThread.ManagedThreadId))
            {
                throw new InvalidOperationException("Unable to access ConcurrentList members from within a " +
                    $"{nameof(ForAsync)} or {nameof(ForEachAsync)} loop.");
            }
        }
    }
}
