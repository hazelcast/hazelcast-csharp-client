using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class AsyncTests
    {
        // https://stackoverflow.com/questions/19481964/calling-taskcompletionsource-setresult-in-a-non-blocking-manner
        // http://blog.stephencleary.com/2012/12/dont-block-in-asynchronous-code.html

        // taskCompletionSource.SetResult() scheduled with .ExecuteSynchronously = duh, beware!

        [Test]
        public async Task CompletionSourceCompletesResultSynchronously()
        {
            var steps = new Steps();

            steps.Add("start");

            var taskCompletionSource = new TaskCompletionSource<int>();
            var task = Task.Run(async () =>
            {
                steps.Add("task.start");
                await Task.Delay(2000).CAF();
                steps.Add("task.complete");
                taskCompletionSource.SetResult(42); // this is NOT fire-and-forget !!
                steps.Add("task.end");
            });

            steps.Add("wait");
            await taskCompletionSource.Task.CAF();
            steps.Add("end");

            await Task.Delay(100).CAF();

            Console.WriteLine(steps);

            var threadSetResult = steps.GetThreadId("task.complete");
            var threadCompleted = steps.GetThreadId("end");
            Assert.AreEqual(threadCompleted, threadSetResult);

            var indexTaskEnd = steps.GetIndex("task.end");
            var indexEnd = steps.GetIndex("end");
            Assert.Greater(indexTaskEnd, indexEnd);
        }

        [Test]
        public async Task CompletionSourceCompletesResultAsynchronously()
        {
            var steps = new Steps();

            steps.Add("start");

            var taskCompletionSource = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var task = Task.Run(async () =>
            {
                steps.Add("task.start");
                await Task.Delay(2000).CAF();
                steps.Add("task.complete");
                taskCompletionSource.SetResult(42); // this is NOT fire-and-forget !!
                steps.Add("task.end");
            });

            steps.Add("wait");
            await taskCompletionSource.Task.CAF();
            steps.Add("end");

            await Task.Delay(100).CAF();

            Console.WriteLine(steps);

            var threadSetResult = steps.GetThreadId("task.complete");
            var threadTaskEnd = steps.GetThreadId("task.end");
            Assert.AreEqual(threadTaskEnd, threadSetResult);

            var threadTaskCompleted = steps.GetThreadId("wait");
            Assert.AreNotEqual(threadSetResult, threadTaskCompleted);

            var indexTaskEnd = steps.GetIndex("task.end");
            var indexEnd = steps.GetIndex("end");
            Assert.Less(indexTaskEnd, indexEnd);
        }

        [Test]
        public async Task CompletionSourceTest()
        {
            var sources = new[]
            {
                new TaskCompletionSource<object>(),
                new TaskCompletionSource<object>(),
                new TaskCompletionSource<object>(),
                new TaskCompletionSource<object>(),
                new TaskCompletionSource<object>(),
            };

            var wait = Task.Run(async () => await Task.WhenAll(sources.Select(x => x.Task)).CAF());

            static void Throw(int j)
                => throw new Exception("bang_" + j);

            // can set exception on many tasks without problems
            var i = 0;
            foreach (var source in sources)
            {
                await Task.Delay(100).CAF();

                //source.SetException(new Exception("bang_" + i));
                try
                {
                    Throw(i);
                }
                catch (Exception e)
                {
                    source.SetException(e);
                }

                i++;

                await Task.Delay(100).CAF();

                // wait becomes Faulted only when all tasks have completed
                var expected = i == 5 ? TaskStatus.Faulted : TaskStatus.WaitingForActivation;
                Assert.AreEqual(expected, wait.Status);
            }

            Assert.AreEqual(5, i);

            // throws the "bang" exception
            Assert.ThrowsAsync<Exception>(async () => await wait.CAF());

            i = 0;
            foreach (var source in sources)
            {
                Console.WriteLine("SOURCE_" + i++);
                Assert.IsTrue(source.Task.IsFaulted);
                var e = source.Task.Exception;
                Console.WriteLine(e);
            }

            // is faulted with only the first one
            // WhenAll only captures the first exception
            Assert.IsTrue(wait.IsFaulted);
            var exception = wait.Exception;
            Assert.AreEqual(1, exception.InnerExceptions.Count);

            // there is only one
            foreach (var e in exception.InnerExceptions)
                Console.WriteLine(e);
        }

        [Test]
        public void DefaultTimeSpanIsZero()
        {
            Assert.AreEqual(TimeSpan.Zero, default(TimeSpan));
        }

        [Test]
        [Timeout(20_000)]
        public void TokenSourceWithTimeoutTimesOut()
        {
            const int timeout = 1_000;
            const int delay = 2_000;

            using var generalCancellation = new CancellationTokenSource();
            var thisCancellation = generalCancellation.WithTimeout(timeout);

            // we get a TaskCancelledException
            Assert.ThrowsAsync<TaskCanceledException>(async () => await Task
                .Delay(delay, thisCancellation.Token));
            thisCancellation.Dispose();
            Assert.IsFalse(generalCancellation.IsCancellationRequested);
        }

        [Test]
        [Timeout(20_000)]
        public void TokenSourceWithTimeoutTimesOutWithProperException1()
        {
            const int timeout = 1_000;
            const int delay = 2_000;

            using var generalCancellation = new CancellationTokenSource();
            var thisCancellation = generalCancellation.WithTimeout(timeout);

            // better: if we handle the timeout, we get a TimeoutException
            Assert.ThrowsAsync<TimeoutException>(async () => await Task
                .Delay(delay, thisCancellation.Token)
                .OrTimeout(thisCancellation));
            Assert.IsFalse(generalCancellation.IsCancellationRequested);
        }

        [Test]
        [Timeout(20_000)]
        public void TokenSourceWithTimeoutTimesOutWithProperException2()
        {
            const int timeout = 1_000;
            const int delay = 2_000;

            static async Task RunAsync(CancellationToken cancellationToken, int i)
            {
                if (i == 0)
                    await Task.Delay(delay, cancellationToken).CAF();
                else
                    await RunAsync(cancellationToken, i - 1).CAF();
            }

            using var generalCancellation = new CancellationTokenSource();
            var thisCancellation = generalCancellation.WithTimeout(timeout);

            // even better: can get the full stack trace
            Assert.ThrowsAsync<TimeoutException>(async () => await
                RunAsync(thisCancellation.Token, 4)
                .OrTimeout(thisCancellation));
            Assert.IsFalse(generalCancellation.IsCancellationRequested);
        }

        [Test]
        [Timeout(20_000)]
        public void TokenSourceWithTimeoutCancels()
        {
            const int timeout = 5_000;
            const int delay = 1_000;
            const int cancel = 500;

            using var generalCancellation = new CancellationTokenSource(cancel);
            var thisCancellation = generalCancellation.WithTimeout(timeout);

            // if we actually cancel, we get a TaskCanceledException
            Assert.ThrowsAsync<TaskCanceledException>(async () => await Task
                .Delay(delay, thisCancellation.Token)
                .OrTimeout(thisCancellation));
            Assert.IsTrue(generalCancellation.IsCancellationRequested);
            Assert.IsTrue(thisCancellation.IsCancellationRequested);
        }

        [Test]
        [Timeout(20_000)]
        public async Task TokenSourceWithTimeoutCompletes()
        {
            const int timeout = 2_000;
            const int delay = 1_000;

            using var generalCancellation = new CancellationTokenSource();
            var thisCancellation = generalCancellation.WithTimeout(timeout);

            await Task.Delay(delay, thisCancellation.Token).OrTimeout(thisCancellation).CAF();

            Assert.IsFalse(generalCancellation.IsCancellationRequested);
            // thisCancellation has been disposed
        }

        [Test]
        [Timeout(20_000)]
        public async Task TokenSourceWithTimeoutThrows()
        {
            const int timeout = 2_000;
            const int delay = 100;

            static async Task RunAsync(CancellationToken cancellationToken)
            {
                await Task.Delay(delay, cancellationToken).CAF();
                throw new Exception("bang");
            }

            using var generalCancellation = new CancellationTokenSource();
            var thisCancellation = generalCancellation.WithTimeout(timeout);

            try
            {
                // if the task throws before timeout, we get the real proper exception
                await RunAsync(thisCancellation.Token).OrTimeout(thisCancellation).CAF();
            }
            catch (Exception e)
            {
                Assert.AreEqual("bang", e.Message);
            }

            Assert.IsFalse(generalCancellation.IsCancellationRequested);
            // thisCancellation has been disposed
        }

        [Test]
        [Timeout(20_000)]
        public async Task DelayCancel()
        {
            var cancellation = new CancellationTokenSource(1_000);
            Assert.ThrowsAsync<TaskCanceledException>(async () => await Task.Delay(2_000, cancellation.Token).CAF());
            await Task.Delay(100, CancellationToken.None).CAF();

            // TaskCancelledException inherits from OperationCancelledException
            cancellation = new CancellationTokenSource(1_000);
            try
            {
                await Task.Delay(2_000, cancellation.Token).CAF();
            }
            catch (OperationCanceledException)
            { }
        }

        [Test]
        [Timeout(20_000)]
        public async Task WaitCancel()
        {
            var semaphore = new SemaphoreSlim(0);

            var cancellation = new CancellationTokenSource(1_000);
            Assert.ThrowsAsync<OperationCanceledException>(async () => await semaphore.WaitAsync(cancellation.Token).CAF());
            await Task.Delay(100, CancellationToken.None).CAF();
        }

        [Test]
        [Timeout(20_000)]
        public async Task WhenAnyCancel()
        {
            var semaphore = new SemaphoreSlim(0);

            var cancellation = new CancellationTokenSource(1_000);

            var task1 = Task.Delay(2_000, cancellation.Token);
            var task2 = semaphore.WaitAsync(cancellation.Token);

            var t = await Task.WhenAny(task1, task2); // does not throw
            await Task.Delay(100, CancellationToken.None).CAF();

            Assert.IsTrue(t.IsCompleted);
            Assert.IsTrue(t.IsCanceled);
            Assert.IsTrue(task1.IsCanceled);
            Assert.IsTrue(task2.IsCanceled);
        }

        [Test]
        [Timeout(20_000)]
        public async Task WhenAllCancel()
        {
            var semaphore = new SemaphoreSlim(0);

            var cancellation = new CancellationTokenSource(1_000);

            var task1 = Task.Delay(2_000, cancellation.Token);
            var task2 = semaphore.WaitAsync(cancellation.Token);

            Assert.ThrowsAsync<TaskCanceledException>(async () => await Task.WhenAll(task1, task2).CAF());
            await Task.Delay(100, CancellationToken.None).CAF();

            Assert.IsTrue(task1.IsCanceled);
            Assert.IsTrue(task2.IsCanceled);
        }

        private static void WriteContext(string n)
        {
            Console.WriteLine($"{n}: [{Thread.CurrentThread.ManagedThreadId}] {AsyncContext.CurrentContext.Id}");
        }

        private static Task WriteContextAsync(string n)
        {
            WriteContext(n);
            return Task.CompletedTask;
        }

        [Test]
        public void AsyncContextWhenNonAsync()
        {
            WriteContext("1");
            WriteContext("2");
        }

        [Test]
        public async Task AsyncContext1()
        {
            await WriteContextAsync("1");
            await WriteContextAsync("2");
        }

        [Test]
        public async Task AsyncContext2()
        {
            await WriteContextAsync("1");
            await Task.Delay(100);
            Console.WriteLine("!!");
            await Task.Delay(100);
            await WriteContextAsync("2");
        }

        [Test]
        public async Task AsyncContextWhenAsync()
        {
            await AsyncContextWhenAsyncSub("x1")
                .ContinueWith(x =>
                {
                    var id = AsyncContext.CurrentContext.Id;
                    var t = Thread.CurrentThread.ManagedThreadId;
                    Console.WriteLine($"x3 [{t}] {id}");
                })
                .ContinueWith(async x => await AsyncContextWhenAsyncSub("x2"));

            // no value by default = the first task will set one,
            // but that value will *not* be seen by any continuation
            await AsyncContextWhenAsyncSub("y1")
                .ContinueWith(async x => await AsyncContextWhenAsyncSub("y2"));

            // first time we get a context = creates a context

            var id1 = AsyncContext.CurrentContext.Id;
            var t1 = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"1: [{t1}] {id1}");

            await Task.Yield();

            var id2 = AsyncContext.CurrentContext.Id;
            var t2 = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"2: [{t2}] {id2}");

            Assert.AreEqual(id1, id2);
            //Assert.AreNotEqual(t1, t2);

            long id3 = 0, id4 = 0;
            int t3 = 0, t4 = 0;
            /*
            await Task.Run(() =>
            {
                // the context, being async local, flows here
                // we need to explicitly indicate that we are starting something new
                AsyncContext.NewContext();

                // FIXME: that cannot work - does not flow to continuations
                // changing it... well it's all the *same* context
                // see https://github.com/StephenCleary/AsyncEx/blob/master/src/Nito.AsyncEx.Context/AsyncContext.cs
                // but... that is quite a beast... to implement a *new* thing

                // however, *all* map operations need this "context id" so we cannot
                // require that ppl pass it along to all operations, it has to be
                // an "ambiant" thing

                id3 = AsyncContext.CurrentContext.Id;
                t3 = Thread.CurrentThread.ManagedThreadId;
                Console.WriteLine($"[{t3}] {id3}");
            }).ContinueWith(t =>
            {
                id4 = AsyncContext.CurrentContext.Id;
                t4 = Thread.CurrentThread.ManagedThreadId;
                Console.WriteLine($"[{t4}] {id4}");
            }, TaskContinuationOptions.RunContinuationsAsynchronously);
            */

            await AsyncContextWhenAsyncSub("a");
            await AsyncContextWhenAsyncSub("b");
            //AsyncContext.NewContext();
            await AsyncContext.RunDetached(async () =>
            {
                await AsyncContextWhenAsyncSub("c").ContinueWith(async x => await AsyncContextWhenAsyncSub("c2"));
            });

            Task task = null;
            Console.WriteLine("--");
            await AsyncContext.RunDetached(() => task = AsyncContextWhenAsyncSub("z"));
            Console.WriteLine("--");
            await task;

            AsyncContext.RunDetached(() =>
            {
                Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    await AsyncContextWhenAsyncSub("d");
                });
            });

            var id5 = AsyncContext.CurrentContext.Id;
            var t5 = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"5: [{t5}] {id5}");

            Assert.AreEqual(id1, id5);
            Assert.AreNotEqual(id1, id3);
            Assert.AreEqual(id3, id4);

            //Assert.AreNotEqual(t3, t4);

            await Task.Delay(2000);
        }

        private async Task AsyncContextWhenAsyncSub(string n)
        {
            long id3 = 0, id4 = 0;
            int t3 = 0, t4 = 0;

            id3 = AsyncContext.CurrentContext.Id;
            t3 = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"3{n}: [{t3}] {id3}");

            async Task f()
            {
                Console.WriteLine($"3{n}!: [{Thread.CurrentThread.ManagedThreadId}] {AsyncContext.CurrentContext.Id}");
                await Task.Yield();
            }

            await Task.Delay(500);
            await f();
            await Task.Delay(500);

            id3 = AsyncContext.CurrentContext.Id;
            t3 = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"3{n}: [{t3}] {id3}");
        }

        private class Steps
        {
            private readonly ConcurrentQueue<Step> _steps = new ConcurrentQueue<Step>();

            public void Add(string message)
                => _steps.Enqueue(new Step(message));

            public int GetThreadId(string message)
                => _steps.FirstOrDefault(x => x.Message == message)?.ManagedThreadId ?? 0;

            public int GetIndex(string message)
            {
                var i = 0;
                foreach (var x in _steps)
                {
                    if (x.Message == message) return i;
                    i++;
                }

                return -1;
            }

            public override string ToString()
            {
                var text = new StringBuilder();
                foreach (var step in _steps)
                {
                    if (text.Length > 0) text.Append(Environment.NewLine);
                    text.Append(step);
                }

                return text.ToString();
            }
        }

        private class Step
        {
            public Step(string message)
            {
                ManagedThreadId = Thread.CurrentThread.ManagedThreadId;
                Message = message;
            }

            public int ManagedThreadId { get; }

            public string Message { get; }

            public override string ToString()
                => $"[{ManagedThreadId:00}] {Message}";
        }
    }
}
