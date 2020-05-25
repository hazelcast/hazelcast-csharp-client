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
                await Task.Delay(2000);
                steps.Add("task.complete");
                taskCompletionSource.SetResult(42); // this is NOT fire-and-forget !!
                steps.Add("task.end");
            });

            steps.Add("wait");
            await taskCompletionSource.Task;
            steps.Add("end");

            await Task.Delay(100);

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
                await Task.Delay(2000);
                steps.Add("task.complete");
                taskCompletionSource.SetResult(42); // this is NOT fire-and-forget !!
                steps.Add("task.end");
            });

            steps.Add("wait");
            await taskCompletionSource.Task;
            steps.Add("end");

            await Task.Delay(100);

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

            var wait = Task.Run(async () => await Task.WhenAll(sources.Select(x => x.Task)));

            void Throw(int j)
                => throw new Exception("bang_" + j);

            // can set exception on many tasks without problems
            var i = 0;
            foreach (var source in sources)
            {
                await Task.Delay(100);

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

                await Task.Delay(100);

                // wait becomes Faulted only when all tasks have completed
                var expected = i == 5 ? TaskStatus.Faulted : TaskStatus.WaitingForActivation;
                Assert.AreEqual(expected, wait.Status);
            }

            Assert.AreEqual(5, i);

            // throws the "bang" exception
            Assert.ThrowsAsync<Exception>(async () => await wait);

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

        // TODO: consider removing this code
        /*
        [Test]
        [Timeout(20_000)]
        public async Task TaskWithTimeoutCompletes()
        {
            const int timeout = 10_000;
            const int delay = 2_000;

            var task = Task.Delay(delay, CancellationToken.None)
                .ContinueWith(_ => 2, CancellationToken.None);
            var i = await task.WithTimeout(timeout);
            Assert.AreEqual(2, i);
        }

        [Test]
        [Timeout(20_000)]
        public async Task TaskWithTimeoutTimesOut()
        {
            const int timeout = 1_000;
            const int delay = 2_000;
            var isCompleted = new SemaphoreSlim(0);

            var task = Task.Delay(delay)
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully())
                        isCompleted.Release();
                }, CancellationToken.None);

            Assert.ThrowsAsync<TimeoutException>(async () => await task.WithTimeout(timeout));
            await isCompleted.WaitAsync(CancellationToken.None);
        }

        [Test]
        [Timeout(20_000)]
        public async Task TaskWithTimeoutTimesOutAndCancels()
        {
            const int timeout = 1_000;
            const int delay = 2_000;
            var isCancelled = new SemaphoreSlim(0);

            var taskCancel = new CancellationTokenSource();
            var task = Task.Delay(delay, taskCancel.Token)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        isCancelled.Release();
                }, CancellationToken.None);

            Assert.ThrowsAsync<TimeoutException>(async () => await task.WithTimeout(timeout, taskCancel));
            await isCancelled.WaitAsync(CancellationToken.None);
        }

        [Test]
        [Timeout(20_000)]
        public async Task TaskWithTimeoutThrows()
        {
            const int timeout = 1_000;

            var task = Task.Delay(100).ContinueWith(t => throw new Exception("bang"));

            Assert.ThrowsAsync<Exception>(async () => await task.WithTimeout(timeout));

            await Task.Delay(100);
        }

        [Test]
        [Timeout(20_000)]
        public async Task TaskWithTimeoutThrowsWhenCancelled()
        {
            const int timeout = 1_000;
            var semaphore = new SemaphoreSlim(0);
            var hasThrown = new SemaphoreSlim(0);
            Exception exception = null;

            async Task RunTask(CancellationToken token)
            {
                await Task.Delay(100, CancellationToken.None);
                try
                {
                    // this throws when token is cancelled
                    await semaphore.WaitAsync(token);
                }
                catch (Exception e)
                {
                    exception = e;
                    hasThrown.Release();
                    throw;
                }
            }

            var taskCancel = new CancellationTokenSource();
            var task = RunTask(taskCancel.Token);

            Assert.ThrowsAsync<TimeoutException>(async () => await task.WithTimeout(timeout, taskCancel));

            await hasThrown.WaitAsync(CancellationToken.None);
            await Task.Delay(100, CancellationToken.None);
            Assert.IsInstanceOf<OperationCanceledException>(exception);
        }
        */

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
                    await Task.Delay(delay, cancellationToken);
                else
                    await RunAsync(cancellationToken, i - 1);
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

            await Task.Delay(delay, thisCancellation.Token).OrTimeout(thisCancellation);

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
                await Task.Delay(delay, cancellationToken);
                throw new Exception("bang");
            }

            using var generalCancellation = new CancellationTokenSource();
            var thisCancellation = generalCancellation.WithTimeout(timeout);

            try
            {
                // if the task throws before timeout, we get the real proper exception
                await RunAsync(thisCancellation.Token).OrTimeout(thisCancellation);
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
            Assert.ThrowsAsync<TaskCanceledException>(async () => await Task.Delay(2_000, cancellation.Token));
            await Task.Delay(100, CancellationToken.None);

            // TaskCancelledException inherits from OperationCancelledException
            cancellation = new CancellationTokenSource(1_000);
            try
            {
                await Task.Delay(2_000, cancellation.Token);
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
            Assert.ThrowsAsync<OperationCanceledException>(async () => await semaphore.WaitAsync(cancellation.Token));
            await Task.Delay(100, CancellationToken.None);
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
            await Task.Delay(100, CancellationToken.None);

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

            Assert.ThrowsAsync<TaskCanceledException>(async () => await Task.WhenAll(task1, task2));
            await Task.Delay(100, CancellationToken.None);

            Assert.IsTrue(task1.IsCanceled);
            Assert.IsTrue(task2.IsCanceled);
        }

        private void Log(string msg)
        {
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId:00}] " + msg);
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
