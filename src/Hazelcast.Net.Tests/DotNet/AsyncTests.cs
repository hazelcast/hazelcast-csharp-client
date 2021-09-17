// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    [Timeout(20_000)]
    public class AsyncTests
    {
        // https://stackoverflow.com/questions/19481964/calling-taskcompletionsource-setresult-in-a-non-blocking-manner
        // http://blog.stephencleary.com/2012/12/dont-block-in-asynchronous-code.html
        //
        // taskCompletionSource.SetResult() scheduled with .ExecuteSynchronously = duh, beware!

        [TestCase(true)]
        [TestCase(false)]
        public async Task CompletionSourceCompletesResultAsynchronously(bool completeAsync)
        {
            var steps = new Steps();

            var mainThreadId = steps.Add("main.start");

            // create the task completion source - either sync or async
            var options = completeAsync ? TaskCreationOptions.RunContinuationsAsynchronously : TaskCreationOptions.None;
            var taskCompletionSource = new TaskCompletionSource<int>(options);

            var taskThreadId = -1;
            var resultSet = false;
            var task = Task.Run(() =>
            {
                // this has started on another thread
                taskThreadId = steps.Add("task.start");

                // some delay - on the same thread
                // we want some long-enough delay to ensure that we do not SetResult before awaiting the
                // task completion source, thus forcing the await on that task to actually await synchronously
                Thread.Sleep(2000);

                // now set the result, thus unblocking the awaited task
                // all of this happens on the same thread
                Assert.That(steps.Add("task.complete"), Is.EqualTo(taskThreadId));
                taskCompletionSource.SetResult(42);
                resultSet = true;
                Assert.That(steps.Add("task.continue"), Is.EqualTo(taskThreadId));

                // some more delay - on the same thread
                Thread.Sleep(500);
                Assert.That(steps.Add("task.end"), Is.EqualTo(taskThreadId));
            });

            // going to wait on the task until it sets the taskCompletionSource result
            Assert.That(steps.Add("main.wait"), Is.EqualTo(mainThreadId));
            await taskCompletionSource.Task.CfAwait();

            // resume after the task has completed
            var resumedThreadId = steps.Add("main.resume");

            // just to be sure: main and task run on different threads
            Assert.That(taskThreadId, Is.Not.EqualTo(mainThreadId));

            if (completeAsync)
            {
                // if the task completion source completed asynchronously, the the task kept running - we are a different thread
                // which does not have to be mainThreadId since we awaited, but nevertheless - it is not taskThreadId either
                Assert.That(resumedThreadId, Is.Not.EqualTo(taskThreadId));

                // after some delay (on the same thread)
                Thread.Sleep(500);
                Assert.That(Thread.CurrentThread.ManagedThreadId, Is.EqualTo(resumedThreadId));

                // the task has resumed
                Assert.That(resultSet, Is.True);

                // this is OK
                task.GetAwaiter().GetResult(); // 'await' on this thread
            }
            else
            {
                // if the task completion source completed synchronously, then it's the task thread which is running this code
                Assert.That(resumedThreadId, Is.EqualTo(taskThreadId));

                // after some delay (on the same thread)
                Thread.Sleep(500);
                Assert.That(Thread.CurrentThread.ManagedThreadId, Is.EqualTo(taskThreadId));

                // the task will not resume until *we* are done
                Assert.That(resultSet, Is.False);

                // this is NOT OK - hangs, since we are hanging on the task's thread
                //task.GetAwaiter().GetResult(); // 'await' on this thread
                Assert.That(task.Status, Is.EqualTo(TaskStatus.Running));
            }

            var threadId = Thread.CurrentThread.ManagedThreadId;

            // some delay (on the same thread)
            Thread.Sleep(200);
            Assert.That(steps.Add("main.continue"), Is.EqualTo(threadId));

            // some delay (on the same thread)
            Thread.Sleep(200);
            Assert.That(steps.Add("main.continue"), Is.EqualTo(threadId));

            if (completeAsync)
            {
                // some delay (on the same thread)
                Thread.Sleep(200);
                Assert.That(steps.Add("main.end"), Is.EqualTo(threadId));
            }
            else
            {
                // we are still hanging on the task's thread so it cannot complete
                Assert.That(task.Status, Is.EqualTo(TaskStatus.Running));

                // release this thread
                await Task.Yield();

                // this is now OK
                task.GetAwaiter().GetResult(); // 'await' on this thread

                // can be on any thread
                steps.Add("main.end");
            }


            // trace
            Console.WriteLine(steps);

            // what should be traced:
            //
            // synchronous:
            // 00[22] main.start
            // 01[22] main.wait
            // 02[04] task.start
            // 03[04] task.complete
            // 04[04] main.resume    << main resumes and continues on task's thread
            // 05[04] main.continue  << main is holding task's thread
            // 06[04] main.continue
            // 07[04] task.continue  << until it yields, and then the task resumes
            // 08[04] task.end
            // 09[10] main.end
            //
            // asynchronous:
            // 00[22] main.start
            // 01[22] main.wait
            // 02[10] task.start
            // 03[10] task.complete
            // 04[10] task.continue  << task continues and ends on its own thread
            // 05[06] main.resume    << main resumes and continues on another thread
            // 06[10] task.end
            // 07[06] main.continue
            // 08[06] main.continue
            // 09[06] main.end
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

            var wait = Task.Run(async () =>
            {

                return await Task.WhenAll(sources.Select(x => x.Task)).CfAwait();
            });

            static void Throw(int j)
                => throw new Exception("bang_" + j);

            // can set exception on many tasks without problems
            var i = 0;
            foreach (var source in sources)
            {
                await Task.Delay(100).CfAwait();

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

                await Task.Delay(100).CfAwait();

                // wait becomes Faulted only when all tasks have completed
                var expected = i == 5 ? TaskStatus.Faulted : TaskStatus.WaitingForActivation;
                Assert.AreEqual(expected, wait.Status);
            }

            Assert.AreEqual(5, i);

            // throws the "bang" exception
            Assert.ThrowsAsync<Exception>(async () => await wait.CfAwait());

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
        public async Task DelayCancel()
        {
            var cancellation = new CancellationTokenSource(100);
            Assert.ThrowsAsync<TaskCanceledException>(async () => await Task.Delay(2_000, cancellation.Token).CfAwait());
            await Task.Delay(100, CancellationToken.None).CfAwait();

            // TaskCancelledException inherits from OperationCancelledException
            cancellation = new CancellationTokenSource(100);
            try
            {
                await Task.Delay(2_000, cancellation.Token).CfAwait();
            }
            catch (OperationCanceledException)
            { }
        }

        [Test]
        public async Task WaitCancel()
        {
            var semaphore = new SemaphoreSlim(0);

            var cancellation = new CancellationTokenSource(100);
            Assert.ThrowsAsync<OperationCanceledException>(async () => await semaphore.WaitAsync(cancellation.Token).CfAwait());
            await Task.Delay(100, CancellationToken.None).CfAwait();
        }

        [Test]
        public async Task WhenAnyCancel()
        {
            var semaphore = new SemaphoreSlim(0);

            var cancellation = new CancellationTokenSource(100);

            var task1 = Task.Delay(2_000, cancellation.Token);
            var task2 = semaphore.WaitAsync(cancellation.Token);

            var t = await Task.WhenAny(task1, task2); // does not throw
            await Task.Delay(100, CancellationToken.None).CfAwait();

            Assert.IsTrue(t.IsCompleted);
            Assert.IsTrue(t.IsCanceled);
            Assert.IsTrue(task1.IsCanceled);
            Assert.IsTrue(task2.IsCanceled);
        }

        [Test]
        public async Task WhenAllCancel()
        {
            var semaphore = new SemaphoreSlim(0);

            var cancellation = new CancellationTokenSource(100);

            var task1 = Task.Delay(2_000, cancellation.Token);
            var task2 = semaphore.WaitAsync(cancellation.Token);

            Assert.ThrowsAsync<TaskCanceledException>(async () => await Task.WhenAll(task1, task2).CfAwait());
            await Task.Delay(100, CancellationToken.None).CfAwait();

            Assert.IsTrue(task1.IsCanceled);
            Assert.IsTrue(task2.IsCanceled);
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

            public int Add(string message)
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                _steps.Enqueue(new Step(message, threadId));
                return threadId;
            }

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

            public void AssertSameThread(string step1, string step2)
            {
                var thread1 = GetThreadId(step1);
                var thread2 = GetThreadId(step2);
                Assert.That(thread1, Is.EqualTo(thread2), () => $"{step1} thread {thread1} != {step2} thread {thread2}");
            }

            public void AssertNotSameThread(string step1, string step2)
            {
                var thread1 = GetThreadId(step1);
                var thread2 = GetThreadId(step2);
                Assert.That(thread1, Is.Not.EqualTo(thread2), () => $"{step1} thread {thread1} == {step2} thread {thread2}");
            }

            public void AssertOrder(string step1, string step2)
            {
                var index1 = GetIndex(step1);
                var index2 = GetIndex(step2);
                Assert.That(index1, Is.LessThan(index2), () => $"{step1} at {index1} comes after {step2} at {index2}");
            }

            public override string ToString()
            {
                var text = new StringBuilder();
                var i = 0;
                foreach (var step in _steps)
                {
                    if (text.Length > 0) text.Append(Environment.NewLine);
                    text.Append($"{i++:00} ");
                    text.Append(step);
                }

                return text.ToString();
            }
        }

        private class Step
        {
            public Step(string message, int threadId)
            {
                Message = message;
                ManagedThreadId = threadId;
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

        [Test]
        public async Task ContinuationTest()
        {
            var e = new ManualResetEventSlim();

            void ContinuationAction(Task task)
            {
                e.Wait();
            }

            var task = Task.CompletedTask.ContinueWith(ContinuationAction);

            await Task.Delay(100);
            Assert.That(!task.IsCompleted); // ContinueWith returns before running the continuation method
            e.Set();
            await Task.Delay(100);
            await task;
        }

        [Test]
        public void CancellationTokenSourceTest()
        {
            // it is OK to reference the cancellation token of a disposed source

            var source = new CancellationTokenSource();
            var token = source.Token;
            source.Dispose();
            Assert.That(token.IsCancellationRequested, Is.False);

            source = new CancellationTokenSource();
            token = source.Token;
            source.Cancel();
            source.Dispose();
            Assert.That(token.IsCancellationRequested, Is.True);
        }

        [Test]
        public void RegisterOrderTest()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            var i = 0;
            token.Register(() => i++);
            source.Cancel();
            Assert.That(i, Is.EqualTo(1));

            source = new CancellationTokenSource();
            token = source.Token;
            i = 0;
            source.Cancel();
            token.Register(() => i++);
            Assert.That(i, Is.EqualTo(1));
        }

        private class AsyncThing
        {
            private static readonly AsyncLocal<AsyncThing> CurrentAsyncLocal = new AsyncLocal<AsyncThing>();
            private static int _idSequence;

            private AsyncThing()
            {
                Id = _idSequence++;
            }
            public int Id { get; }

            public static bool HasCurrent => CurrentAsyncLocal.Value != null;

            public static AsyncThing Current => CurrentAsyncLocal.Value;

            public static void Ensure()
            {
                if (CurrentAsyncLocal.Value == null) CurrentAsyncLocal.Value = new AsyncThing();
            }

            public static IDisposable New()
            {
                var current = CurrentAsyncLocal.Value;
                var used = new UsedAsyncThing(current);
                CurrentAsyncLocal.Value = new AsyncThing();
                return used;
            }

            private class UsedAsyncThing : IDisposable
            {
                private readonly AsyncThing _asyncThing;

                public UsedAsyncThing(AsyncThing asyncThing)
                {
                    _asyncThing = asyncThing;
                }

                public void Dispose()
                {
                    CurrentAsyncLocal.Value = _asyncThing;
                }
            }
        }

        [Test]
        public async Task AsyncLocalTest()
        {
            Assert.That(AsyncThing.HasCurrent, Is.False);
            AsyncThing.Ensure();
            Assert.That(AsyncThing.HasCurrent);
            var id1 = AsyncThing.Current.Id;
            var id1a = await GetAsyncThingId().ConfigureAwait(false);

            int id2, id2a;
            var sem = new SemaphoreSlim(0);
            Task<int> task;
            using (AsyncThing.New())
            {
                id2 = AsyncThing.Current.Id;
                id2a = await GetAsyncThingId().ConfigureAwait(false);

                task = LongGetAsyncThingId(sem);
            }

            var id3 = AsyncThing.Current.Id;
            var id3a = await GetAsyncThingId().ConfigureAwait(false);

            sem.Release();
            var id4 = await task.ConfigureAwait(false);

            Assert.That(id1, Is.Not.EqualTo(id2));

            Assert.That(id1, Is.EqualTo(id3));

            Assert.That(id1, Is.EqualTo(id1a));
            Assert.That(id2, Is.EqualTo(id2a));
            Assert.That(id3, Is.EqualTo(id3a));

            Assert.That(id4, Is.EqualTo(id2));

            int id5, id5a;
            task = LongGetAsyncThingId(sem);
            using (AsyncThing.New())
            {
                id5 = AsyncThing.Current.Id;
                sem.Release();
                id5a = await task;
            }

            Assert.That(id5, Is.Not.EqualTo(id4)); // different, in a new
            Assert.That(id5a, Is.EqualTo(id1)); // captured when task was started
        }

        public async Task<int> GetAsyncThingId()
        {
            await Task.Yield();
            return AsyncThing.Current.Id;
        }

        public async Task<int> LongGetAsyncThingId(SemaphoreSlim sem)
        {
            await Task.Yield();
            await sem.WaitAsync().ConfigureAwait(false);
            return AsyncThing.Current.Id;
        }

        [Test]
        public void DisposeCancellationSourceTest()
        {
            var cancel = new CancellationTokenSource();
            var token = cancel.Token;
            token.ThrowIfCancellationRequested();

            cancel.Cancel();
            Assert.Throws<OperationCanceledException>(() => token.ThrowIfCancellationRequested());

            // can dispose the source and still use the token
            cancel.Dispose();
            Assert.Throws<OperationCanceledException>(() => token.ThrowIfCancellationRequested());

            cancel = new CancellationTokenSource();
            token = cancel.Token;

            // can dispose the source and still use the token
            cancel.Dispose();
            token.ThrowIfCancellationRequested();

            var cancel1 = new CancellationTokenSource();
            var cancel2 = new CancellationTokenSource();
            var token2 = cancel2.Token;

            cancel = cancel1.LinkedWith(token2);
            token = cancel.Token;

            // can dispose the source that was linked, and still use the token
            cancel1.Dispose();

            token.ThrowIfCancellationRequested();

            cancel2.Cancel();
            Assert.Throws<OperationCanceledException>(() => token.ThrowIfCancellationRequested());
        }

        [Test]
        public async Task AsyncEnumerationTest()
        {
            var v = 0;

            // get an IAsyncEnumerable<> that does not throw when the enumeration is canceled
            var asyncEnumerable = new AsyncEnumerable<int>(new[] { 1, 2, 3, 4 }, throwOnCancel: false);

            // can enumerate
            await foreach (var i in asyncEnumerable)
            {
                Console.WriteLine(v = i);
            }
            Assert.That(v, Is.EqualTo(4));
            Console.WriteLine("/");

            // canceling does not throw
            var cancel = new CancellationTokenSource();
            await foreach (var i in asyncEnumerable.WithCancellation(cancel.Token))
            {
                Console.WriteLine(v = i);
                if (i == 3) cancel.Cancel();
            }
            Assert.That(v, Is.EqualTo(3));
            Console.WriteLine("/");

            // get an IAsyncEnumerable<> that throws when the enumeration is canceled
            asyncEnumerable = new AsyncEnumerable<int>(new[] { 1, 2, 3, 4 });

            // can enumerate
            await foreach (var i in asyncEnumerable)
            {
                Console.WriteLine(v = i);
            }
            Assert.That(v, Is.EqualTo(4));
            Console.WriteLine("/");

            // cancelling throws
            await AssertEx.ThrowsAsync<OperationCanceledException>(async () =>
            {
                cancel = new CancellationTokenSource();
                await foreach (var i in asyncEnumerable.WithCancellation(cancel.Token))
                {
                    Console.WriteLine(v = i);
                    if (i == 3) cancel.Cancel();
                }
            });
            Assert.That(v, Is.EqualTo(3));
            Console.WriteLine("/");

            // can cancel without throwing
            cancel = new CancellationTokenSource();
            await foreach (var i in asyncEnumerable.WithCancellation(throwOnCancel: false, cancel.Token))
            {
                Console.WriteLine(v = i);
                if (i == 3) cancel.Cancel();
            }
            Assert.That(v, Is.EqualTo(3));
            Console.WriteLine("/");

            // repeat with bare enumerable
            var bareEnumerable = new BareAsyncEnumerable<int>(new[] { 1, 2, 3, 4 });

            // can enumerate
            await foreach (var i in bareEnumerable)
            {
                Console.WriteLine(v = i);
            }
            Assert.That(v, Is.EqualTo(4));
            Console.WriteLine("/");

            // cancelling throws
            await AssertEx.ThrowsAsync<OperationCanceledException>(async () =>
            {
                cancel = new CancellationTokenSource();
                await foreach (var i in bareEnumerable.WithCancellation(cancel.Token))
                {
                    Console.WriteLine(v = i);
                    if (i == 3) cancel.Cancel();
                }
            });
            Assert.That(v, Is.EqualTo(3));
            Console.WriteLine("/");

            // can cancel without throwing
            cancel = new CancellationTokenSource();
            await foreach (var i in bareEnumerable.WithCancellation(cancel.Token, false))
            {
                Console.WriteLine(v = i);
                if (i == 3) cancel.Cancel();
            }
            Assert.That(v, Is.EqualTo(3));
            Console.WriteLine("/");

        }

        // provides something that can be async-enumerated without implementing IAsyncEnumerable<>
        // by default, throws if the enumeration is canceled
        public readonly struct BareAsyncEnumerable<T>
        {
            private readonly IReadOnlyList<T> _values;
            private readonly bool _throwOnCancel;
            private readonly CancellationToken _cancellationToken;

            public BareAsyncEnumerable(IReadOnlyList<T> values)
            {
                _values = values;
                _throwOnCancel = true;
                _cancellationToken = default;
            }

            private BareAsyncEnumerable(IReadOnlyList<T> values, bool throwOnCancel, CancellationToken cancellationToken)
            {
                _values = values;
                _throwOnCancel = throwOnCancel;
                _cancellationToken = cancellationToken;
            }

            public BareAsyncEnumerable<T> WithCancellation(CancellationToken cancellationToken, bool throwOnCancel = true)
                => new BareAsyncEnumerable<T>(_values, throwOnCancel, cancellationToken);

            public Enumerator GetAsyncEnumerator() => new Enumerator(_values, _throwOnCancel, _cancellationToken);

            public class Enumerator
            {
                private readonly IReadOnlyList<T> _values;
                private readonly bool _throwOnCancel;
                private readonly CancellationToken _cancellationToken;
                private int _index;

                public Enumerator(IReadOnlyList<T> values, bool throwOnCancel, CancellationToken cancellationToken)
                {
                    _values = values;
                    _throwOnCancel = throwOnCancel;
                    _cancellationToken = cancellationToken;
                    _index = -1;
                }

                public ValueTask<bool> MoveNextAsync()
                {
                    if (_throwOnCancel) _cancellationToken.ThrowIfCancellationRequested();
                    else if (_cancellationToken.IsCancellationRequested) return new ValueTask<bool>(false);

                    return new ValueTask<bool>(++_index < _values.Count);
                }

                public T Current
                {
                    get
                    {
                        if (_cancellationToken.IsCancellationRequested || _index < 0 || _index >= _values.Count) throw new InvalidOperationException();
                        return _values[_index];
                    }
                }

                // this is not *required* but it's invoked if it exists
                public ValueTask DisposeAsync()
                {
                    Console.WriteLine("dispose");
                    return default;
                }
            }
        }

        // provides a true IAsyncEnumerable<> implementation
        // by default, throws if the enumeration is canceled
        public readonly struct AsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly IReadOnlyList<T> _values;
            private readonly bool _throwOnCancel;

            public AsyncEnumerable(IReadOnlyList<T> values, bool throwOnCancel = true)
            {
                _values = values;
                _throwOnCancel = throwOnCancel;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
            {
                return new AsyncEnumerator(_values, _throwOnCancel, cancellationToken);
            }

            private class AsyncEnumerator : IAsyncEnumerator<T>
            {
                private readonly IReadOnlyList<T> _values;
                private readonly bool _throwOnCancel;
                private readonly CancellationToken _cancellationToken;
                private int _index;

                public AsyncEnumerator(IReadOnlyList<T> values, bool throwOnCancel, CancellationToken cancellationToken)
                {
                    _values = values;
                    _throwOnCancel = throwOnCancel;
                    _cancellationToken = cancellationToken;
                    _index = -1;
                }

                public ValueTask<bool> MoveNextAsync()
                {
                    if (_throwOnCancel) _cancellationToken.ThrowIfCancellationRequested();
                    else if (_cancellationToken.IsCancellationRequested) return new ValueTask<bool>(false);

                    return new ValueTask<bool>(++_index < _values.Count);
                }

                public T Current
                {
                    get
                    {
                        if (_cancellationToken.IsCancellationRequested || _index < 0 || _index >= _values.Count) throw new InvalidOperationException();
                        return _values[_index];
                    }
                }

                public ValueTask DisposeAsync()
                {
                    Console.WriteLine("dispose");
                    return default;
                }
            }
        }
    }
}
