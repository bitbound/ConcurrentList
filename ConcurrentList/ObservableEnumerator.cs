using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConcurrentList
{
    public class ObservableEnumerator<T> : IEnumerator<T>
    {
        public event EventHandler Disposed;

        private readonly IList<T> _collection;
        private int _index = 0;

        public ObservableEnumerator(IList<T> collection)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));

            if (!collection.Any())
            {
                new ArgumentException("Collection cannot be empty.");
            }
        }
        public T Current
        {
            get
            {
                if (_index > -1 && _index < _collection.Count)
                {
                    return _collection[_index];
                }
                return default;
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public bool MoveNext()
        {
            if (_index >= _collection.Count)
            {
                return false;
            }
            _index++;
            return true;
        }

        public void Reset()
        {
            _index = 0;
        }
    }
}
