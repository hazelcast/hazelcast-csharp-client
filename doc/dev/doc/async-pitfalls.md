# Asynchronous Pitfalls

This page gathers common pitfalls and issues when migrating synchronous code to an asynchronous programming model. It is not exhaustive, and we enrich it periodically.

## The .NET Framework ASP.NET Issue

A common scenario goes like this: a team had a ASP.NET MVC application, running under .NET Framework 4.8, with a controller using the version 3 of the Hazelcast .NET client and containing method similar to:
```csharp
public ActionResult Index()
{
    var value = GetValue();
    return Content(value);
}

private string GetValue()
{
    var client = HazelcastClient.NewHazelcastClient("path/to/config.xml");
    var map = client.GetMap<string, string>("map-name");
    var value = map.Get("key");
    return value;
}
```

In an attempt to migrate to a newer version of the Hazelcast .NET client, the `GetValue` method is rewritten as:
```csharp
private Task<string> GetValue()
{
    var options = // ...get options...
    await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
    await var map = await client.GetMapAsync<string, string>("map-name");
    var value = await map.GetAsync("key");
    return value;
}
```

And the `Index` method is adjusted as:
```csharp
public ActionResult Index()
{
    var task = GetValue();
    var value = task.Result;
    return Content(value);
}
```

And... the `Index` method hangs and never returns.

### Why It Fails

On classic ASP.NET, controller methods run in a "synchronization context", something that is responsible for scheduling the asynchronous Tasks. In console applications, the synchronization context would schedule Tasks on any thread of the ThreadPool. In ASP.NET applications, the synchronization context is special: each request has its synchronization context, which is bound to one thread at the beginning of the request. The purpose of this was backward compatibility, as people used to rely on their entire request being processed by one single thread. The drawback is that the scheduler can run only one Task at a time, since it only has one thread.

So, here is what happens when the controller's Index method runs:
* The `Index` method calls `GetValue`.
* `GetValue` starts connecting a client by invoking `StartNewClientAsync`.
* `StartNewClientAsync` returns an uncompleted `Task`, indicating that connection is in-progress.
* `GetValue` awaits that `Task`, the context is captured and will be used to continue the `GetValue` method, later. `GetValue` returns an uncompleted `Task`, indicating that it is in-progress
* The `Index` method synchronously block on that `Task` with the `.Result` call. This blocks the context (request) unique thread.
* Eventually, the `Task` for `StartNewClientAsync` will complete. The continuation for `GetValue` (the rest of the method) is now ready to run, and it waits for the synchronization context to schedule that work.
* However, the context is busy waiting (see above) and therefore cannot schedule anything, since it can only execute one thing at a time.
* Deadlock.

This is a classical ASP.NET issue (for instance, you can see it reproduced in [this Gist](https://gist.github.com/leonardochaia/98ce57bcee39c18d88682424a6ffe305)) and explained in details on [this page](https://www.c-sharpcorner.com/article/understanding-synchronization-context-task-configureawait-in-action/) or [this page](https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html).

### One (Dangerous) Fix

There is a way to tell a method to resume on the default ThreadPool-based synchronization context. Consider this line of code:
```csharp
await DoSomething();
```

It will resume execution on the current synchronization context. However, consider this line of code:
```csharp
await DoSomething().ConfigureAwait(false);
```

Here, we are specifically instructing .NET to not resume on the current synchronization context but on the default Thead-Pool one. By adding `ConfigureAwait(false)` to every await statements in `GetValue`, we could hope to fix the issue (it does fix the issue in simple scenarios). But, the thing is, *every single await in the whole chains of calls* needs this. This means that every single await in the Hazelcast .NET Client needs it (they should have it, we have checks for this) and every single await in our dependencies (such as Microsoft's internal code) needs it too (we do not control this).

It turns out that this solutin does *not* work for us. This means that somewhere in the chain of calls, there is at least one single `ConfigureAwait(false)` missing. We do check our own code regularly, and are pretty sure it is correct. But this shows that simply relying on this solution to fix the problem is a dangerous thing.

### The Right Way

The "right way" to fix the issue is to go full-async. That is, turn the `Index` method to async, too:
```csharp
public async Task<ActionResult> Index()
{
    var value = await GetValue().ConfigureAwait(false);
    return Content(value);
}
```

As long as your entire codebase is async, you are safe. Controller actions can be turned to async pretty easily. Likewise, console application's `Main` method can become asynchronous:
```csharp
public static async Task Main(string[] args)
{
    ...
}
```

### The Other Way

In more complex situations, you may hit a point where you *cannot* propagate the asynchronous programming pattern upwards to the top of the chain. Let us say that the `Index` method *has* to remain synchronous, for some reasons. One solution consists in scheduling the asynchronous call on an entirely independent `Task` factory and scheduler, one that is not limited in the way the ASP.NET one is. You will find an example of such a solution below:
```csharp
static class AsyncHelper
{
    private static readonly TaskFactory HelperTaskFactory =
        new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

    public static void Run(Func<Task> func)
        => HelperTaskFactory.StartNew<Task>(func).Unwrap().GetAwaiter().GetResult();

    public static TResult Run<TResult>(Func<Task<TResult>> func)
        => HelperTaskFactory.StartNew<Task<TResult>>(func).Unwrap<TResult>().GetAwaiter().GetResult();
}

public AsyncResult Index()
{
    var value = AsyncHelper.Run(GetValue);
    return Content(value);
}
```

Using this solution *may* have consequences that we are still investigating, but it unlocks a range of situations.