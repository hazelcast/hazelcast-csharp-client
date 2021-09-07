# FAQ

## Could we drop the Async suffix from all asynchronous methods?

No.

See [this post](https://stackoverflow.com/questions/15951774/does-the-use-of-the-async-suffix-in-a-method-name-depend-on-whether-the-async) on StackOverflow, or [this issue](https://github.com/dotnet/runtime/issues/26908#issuecomment-407181532) on GitHub. Or [this tweet](https://twitter.com/Nick_Craver/status/1296527511585726465) by Nick Craver.

We use the `Async` suffix whenever a function returns an async behavior (e.g. `Task` or `ValueTask`) like .NET itself does. The reason for this being that it removes ambiguity and helps stop subtle bugs. For example, say a PR changes this:

```csharp
public string MyFunc() { ... }
```

To this:

```csharp
public Task<string>MyFunc() { ... }
```

If, elsewhere, someone uses the function:

```csharp
var result = MyFunc();
Console.WriteLine(result);
```

... that will still work. But instead of writing a `string`, it will write a `Task`. And it is hard to *see* it. Contrast that with changing to:

```csharp
public Task<string>MyFuncAsync() { ... }
```

Now, the name change *forced* a name change at the call site, so the impact *will* show in a code review. It is a safer, unambiguous version of the change.

## Can we provide synchronous version of the asynchronous methods?

No.

Async-to-Sync such as `client.StartAsync().Wait()` can cause issues such as blocking, dead-locking, starving the ThreadPool etc. This is tricky, and there is no way we can provide a stable implementation of synchronous methods. We'd rather have them happen in user code.