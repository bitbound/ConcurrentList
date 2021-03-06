# ConcurrentList
A simple, lock-based implementation of a thread-safe List&lt;T>.

This is similar to [SynchronizedList&lt;T>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.synchronizedcollection-1?view=netframework-4.8).

This class uses an `ObservableEnumerator` that will lock the list until the enumerator disposed, making it thread-safe.