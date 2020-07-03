# FAQ

# Could we drop the Async suffix from all asynchronous methods?

No. 
https://stackoverflow.com/questions/15951774/does-the-use-of-the-async-suffix-in-a-method-name-depend-on-whether-the-async
https://github.com/dotnet/runtime/issues/26908#issuecomment-407181532

# Can we provide synchronous version of the asynchronous methods?

No.
Async-to-Sync such as `client.OpenAsync().Wait()` can cause issues such as blocking, dead-locking, starving the ThreadPool etc.
We'd rather have them happen in user code.