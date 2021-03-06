# ConcurrentList
A simple, lock-based implementation of a thread-safe List&lt;T>.

This is similar to [SynchronizedList&lt;T>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.synchronizedcollection-1?view=netframework-4.8).

Like a normal list, an exception will be thrown if the collection is modified during enumeration.  Instead, use the `ForAsync` and `ForEachAsync` methods, which are thread-safe.  However, you cannot access other members of the `ConcurrentList<T>` from within these loops.