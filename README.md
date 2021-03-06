# ConcurrentList
A simple, lock-based implementation of a thread-safe List&lt;T>.

This is similar to [SynchronizedList&lt;T>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.synchronizedcollection-1?view=netframework-4.8).

When enumerating the list (e.g. in a `foreach` loop), a copy of the list will be used for enumeration.  This prevents "collection modified" errors, but also means the list may not be up-to-date during enumeration.  This may also cause excess memory usage since it copies the whole list.  Enumerate with caution.

In most cases, you can use the `ForAsync` and `ForEachAsync` methods, which are thread-safe.  However, you cannot directly access other members of the `ConcurrentList<T>` from within these loops.

To make changes to the `ConcurrentList<T>` from within a `ForAsync` or `ForEacAsync` loop, use the `QueueAction` method.  This will queue up actions that will execute after the loop is completed, but before releasing the lock on the list.