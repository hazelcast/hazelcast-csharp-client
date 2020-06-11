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
using System.Runtime.CompilerServices;
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

            /*
            long id3 = 0, id4 = 0;
            int t3 = 0, t4 = 0;

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

            _ = AsyncContext.RunDetached(() =>
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
            //Assert.AreNotEqual(id1, id3);
            //Assert.AreEqual(id3, id4);

            //Assert.AreNotEqual(t3, t4);

            await Task.Delay(2000);
        }

        private static async Task AsyncContextWhenAsyncSub(string n)
        {
            var id = AsyncContext.CurrentContext.Id;
            var t = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"3{n}: [{t}] {id}");

            async Task f()
            {
                Console.WriteLine($"3{n}!: [{Thread.CurrentThread.ManagedThreadId}] {AsyncContext.CurrentContext.Id}");
                await Task.Yield();
            }

            await Task.Delay(500);
            await f();
            await Task.Delay(500);

            id = AsyncContext.CurrentContext.Id;
            t = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"3{n}: [{t}] {id}");
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

        [Test]
        public async Task StackTrace2()
        {
            try
            {
                // can find WrapS1 and WrapThrow2 in the stack trace
                // but of course no trace of WithTimeout = ?
                await TaskEx.WithTimeout(token => WrapS1(() => WrapThrow2<int>(token)), TimeSpan.FromSeconds(1), 1);
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
        public async Task Throwing1()
        {
            // Throw1 throws
            await Throw1();
        }

        [Test]
        public async Task Throwing2()
        {
            // Throw2 is gone from stack traces ;(
            await Throw2();
        }

        [Test]
        public async Task Throwing3()
        {
            // Throw3 shows even if indirectly
            await Throw3();
        }

        [Test]
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
    }
}
