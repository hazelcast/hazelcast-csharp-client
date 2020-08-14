// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Concurrent;
using System.Linq;
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

            var ev = new ManualResetEventSlim();

            steps.Add("start");

            var taskCompletionSource = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var task = Task.Run(async () =>
            {
                await Task.Yield();
                steps.Add("task.start");
                ev.Wait();
                steps.Add("task.complete");
                taskCompletionSource.SetResult(42); // this is NOT fire-and-forget !!
                steps.Add("task.end");
            });

            steps.Add("wait");
            await Task.Delay(200).CAF();
            ev.Set();
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
        public async Task DelayCancel()
        {
            var cancellation = new CancellationTokenSource(100);
            Assert.ThrowsAsync<TaskCanceledException>(async () => await Task.Delay(2_000, cancellation.Token).CAF());
            await Task.Delay(100, CancellationToken.None).CAF();

            // TaskCancelledException inherits from OperationCancelledException
            cancellation = new CancellationTokenSource(100);
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

            var cancellation = new CancellationTokenSource(100);
            Assert.ThrowsAsync<OperationCanceledException>(async () => await semaphore.WaitAsync(cancellation.Token).CAF());
            await Task.Delay(100, CancellationToken.None).CAF();
        }

        [Test]
        [Timeout(20_000)]
        public async Task WhenAnyCancel()
        {
            var semaphore = new SemaphoreSlim(0);

            var cancellation = new CancellationTokenSource(100);

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

            var cancellation = new CancellationTokenSource(100);

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
            // TODO: use manual reset events instead of timers

            Console.WriteLine("There is no ambient context, so each task + continuation creates its own.");

            long x1 = 0, x2 = 0, x3 = 0;

            await AsyncContextWhenAsyncSub("x1")
                .ContinueWith(x =>
                {
                    x1 = x.Result;
                    x2 = AsyncContext.CurrentContext.Id;
                    Console.WriteLine($"-x2: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");
                })
                .ContinueWith(async x => x3 = await AsyncContextWhenAsyncSub("x3"));

            Assert.AreNotEqual(x1, x2);
            Assert.AreNotEqual(x1, x3);
            Assert.AreNotEqual(x2, x3);

            await Task.Delay(500); // complete above code
            Console.WriteLine("--");


            Console.WriteLine("If we ensure a context (as HazelcastClient does) then we have a context that flows.");

            // first time we get a context = creates a context
            var z1 = AsyncContext.CurrentContext.Id;
            Console.WriteLine($"-z1: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");

            await Task.Yield();

            var z2 = AsyncContext.CurrentContext.Id;
            Console.WriteLine($"-z2: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");

            Assert.Greater(z1, x3);
            Assert.AreEqual(z1, z2);

            await Task.Delay(500); // complete above code
            Console.WriteLine("--");

            Console.WriteLine("Running detached code creates a new context, which flows.");

            long c1 = 0, c2 = 0, c4 = 0;

            var cx = await TaskEx.WithNewContext(async () =>
            {
                return await AsyncContextWhenAsyncSub("c1").ContinueWith(async x =>
                {
                    c1 = x.Result;
                    c2 = await AsyncContextWhenAsyncSub("c2");
                    return c2;
                }).Unwrap();
            });

            await Task.Delay(500); // complete above code
            Console.WriteLine("-cx: [  ] " + cx);
            Console.WriteLine("--");

            Task<long> task = null;
            await TaskEx.WithNewContext(() => task = AsyncContextWhenAsyncSub("c3"));

            Console.WriteLine("--");
            var c3 = await task;

            _ = TaskEx.WithNewContext(async () =>
            {
                await Task.Delay(100);
                c4 = await AsyncContextWhenAsyncSub("c4");
            });

            await Task.Delay(500); // complete above code

            Assert.AreEqual(cx, c1);
            Assert.Greater(c1, z2);
            Assert.AreEqual(c1, c2);
            Assert.Greater(c3, c1);
            Assert.Greater(c4, c3);

            Console.WriteLine("--");

            Console.WriteLine("This operation is still in the same context");
            var z3 = AsyncContext.CurrentContext.Id;
            Console.WriteLine($"-z3: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");
            Assert.AreEqual(z1, z3);

            await Task.Delay(500);
        }

        private static async Task<long> AsyncContextWhenAsyncSub(string n)
        {
            var id = AsyncContext.CurrentContext.Id;
            Console.WriteLine($">{n}: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");

            async Task F(long i)
            {
                Assert.AreEqual(i, AsyncContext.CurrentContext.Id);
                Console.WriteLine($">{n}:   [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");
                await Task.Delay(10);
                Assert.AreEqual(i, AsyncContext.CurrentContext.Id);
                Console.WriteLine($">{n}:   [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");
            }

            await Task.Delay(10);
            await F(id);
            await Task.Delay(10);

            Assert.AreEqual(id, AsyncContext.CurrentContext.Id);
            Console.WriteLine($">{n}: [{Thread.CurrentThread.ManagedThreadId:00}] {AsyncContext.CurrentContext.Id}");

            return id;
        }

        [Test]
        public async Task AwaitableSuccess()
        {
            static async Task<int> ReturnIntAsync(int value)
            {
                await Task.Yield();
                return value;
            }

            static Task<(string, TResult)> Execute<TResult>(string thingId, Func<Task<TResult>> function)
            {
                return function().ContinueWith(x => (thingId, x.Result));
            }

            var (thingId, value) = await Execute("a", () => ReturnIntAsync(42));

            Assert.AreEqual("a", thingId);
            Assert.AreEqual(42, value);
        }

        [Test]
        public async Task AwaitableFault()
        {
            static async Task<int> ReturnIntAsync(int value)
            {
                await Task.Yield();
                throw new Exception("bang");
            }

            static Task<(string, TResult)> Execute<TResult>(string thingId, Func<Task<TResult>> function)
            {
                return function().ContinueWith(x => (thingId, x.Result), TaskContinuationOptions.ExecuteSynchronously);
            }

            try
            {
                var (thingId, value) = await Execute("a", () => ReturnIntAsync(42));
                Assert.Fail();
            }
            catch (AggregateException ae)
            {
                Assert.AreEqual(1, ae.InnerExceptions.Count);
                var e = ae.InnerExceptions[0];
                Assert.AreEqual(typeof(Exception), e.GetType());
                Assert.AreEqual("bang", e.Message);
            }
        }

        [Test]
        public async Task AwaitableCancel()
        {
            static async Task<int> ReturnIntAsync(int value, CancellationToken cancellationToken)
            {
                await Task.Delay(2_000, cancellationToken);
                return 42;
            }

            static Task<(string, TResult)> Execute<TResult>(string thingId, Func<CancellationToken, Task<TResult>> function, CancellationToken cancellationToken)
            {
                return function(cancellationToken).ContinueWith(x => (thingId, x.Result), TaskContinuationOptions.ExecuteSynchronously);
            }

            var cancellation = new CancellationTokenSource(1_000);

            try
            {
                var (thingId, value) = await Execute("a", token => ReturnIntAsync(42, token), cancellation.Token);
            }
            catch (AggregateException ae)
            {
                Assert.AreEqual(1, ae.InnerExceptions.Count);
                var e = ae.InnerExceptions[0];
                Assert.AreEqual(typeof (TaskCanceledException), e.GetType());
            }
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

        [Test]
        public async Task StackTrace1()
        {
            try
            {
                // shows WrapS1 and WrapS2 in the stack trace
                //await WrapS1(() => WrapS2(WrapThrow<int>));

                // WrapD1 and WrapD2 missing from the stack trace
                await WrapD1(() => WrapD2(WrapThrow<int>));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task<T> WrapS1<T>(Func<Task<T>> function)
            => await function();

        public Task<T> WrapD1<T>(Func<Task<T>> function)
            => function();

        public Task<T> WrapD2<T>(Func<Task<T>> function)
            => function();

        public async Task<T> WrapThrow<T>()
        {
            await Task.Yield();
            throw new Exception("bang");
        }

        public async Task<T> WrapThrow2<T>(CancellationToken token)
        {
            await Task.Delay(3_000, token);
            if (token.IsCancellationRequested) return default;
            throw new Exception("bang");
        }

        [Test]
        [Explicit("throws - just to see how NUnit reports exceptions")]
        public async Task Throwing1()
        {
            // Throw1 throws
            await Throw1();
        }

        [Test]
        [Explicit("throws - just to see how NUnit reports exceptions")]
        public async Task Throwing2()
        {
            // Throw2 is gone from stack traces ;(
            await Throw2();
        }

        [Test]
        [Explicit("throws - just to see how NUnit reports exceptions")]
        public async Task Throwing3()
        {
            // Throw3 shows even if indirectly
            await Throw3();
        }

        [Test]
        [Explicit("throws - just to see how NUnit reports exceptions")]
        public async Task Throwing4()
        {
            // Throw4 ?
            var task = Throw4();
            Console.WriteLine("here");
            await task; // throws here
        }

        // test handling of exception before the first await in a method?
        public Task Throw()
        {
            throw new Exception("bang");
        }

        public async Task Throw1()
        {
            await Throw();
        }

        public Task Throw2()
        {
            return Throw();
        }

        public Task Throw3()
        {
            try
            {
                return Throw();
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        // if the method is 'async' then can throw anytime, will work
        // the issue is for methods which return a Task without being 'async'
        public async Task Throw4()
        {
            await Task.Yield();
            //await Task.Delay(100);
            throw new Exception("bang");
            //await Throw();
        }

        [Test]
        public async Task CreateTask()
        {
            var e = new ManualResetEventSlim();
            var state = new SomeState();
            var task = DoSomethingAsync(state, e);
            Assert.That(state.Count, Is.EqualTo(1));
            Assert.That(task.IsCompletedSuccessfully, Is.False);
            e.Set();
            await task;
            Assert.That(state.Count, Is.EqualTo(2));
            Assert.That(task.IsCompletedSuccessfully, Is.True);
        }

        public class SomeState
        {
            public int Count { get; set; }
        }

        public async Task DoSomethingAsync(SomeState state, ManualResetEventSlim e)
        {
            state.Count += 1;

            // code before this line executes synchronously when DoSomethingAsync is invoked,
            // and before the Task instance is returned - so it is pausing the caller.

            await Task.Yield();

            // code after this line executes after the Task instance is returned, and runs
            // concurrently with the caller.

            e.Wait();
            state.Count += 1;

            // when we reach the last line of the method, 'await' returns to the caller.
        }

        [Test]
        public async Task ResultOrCancel1()
        {
            var cancellation = new CancellationTokenSource();
            var completion = new TaskCompletionSource<int>();
            cancellation.Token.Register(() => completion.TrySetCanceled());
            completion.TrySetResult(42);
            cancellation.Cancel();
            var v = await completion.Task;
            Assert.That(v, Is.EqualTo(42));
        }

        [Test]
        public void ResultOrCancel2()
        {
            var cancellation = new CancellationTokenSource();
            var completion = new TaskCompletionSource<int>();
            cancellation.Token.Register(() => completion.TrySetCanceled());
            cancellation.Cancel();
            completion.TrySetResult(42);
            Assert.ThrowsAsync<TaskCanceledException>(async () => _ = await completion.Task);
        }
    }
}
