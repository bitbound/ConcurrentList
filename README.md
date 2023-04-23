# ConcurrentList
A simple, lock-based implementation of a thread-safe List&lt;T>.

This is similar to [SynchronizedList&lt;T>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.synchronizedcollection-1?view=netframework-4.8).

When enumerating the list (e.g. in a `foreach` loop), a copy of the list will be used for enumeration.  This prevents "collection modified" errors, but also means the list may not be up-to-date during enumeration.  This may also cause excess memory usage since it copies the whole list.  Enumerate with caution.