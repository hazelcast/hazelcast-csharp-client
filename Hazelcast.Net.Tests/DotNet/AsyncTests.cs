using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public async Task Test1()
        {
            Log("start");
            var taskCompletionSource = new TaskCompletionSource<int>();
            var task = Task.Run(async () =>
            {
                Log("wait");
                await Task.Delay(2000);
                Log("complete");
                taskCompletionSource.SetResult(42); // this is NOT fire-and-forget !!
                Log("done");
            });
            Log("wait completion");

            await taskCompletionSource.Task;
            //await Task.Delay(2000); // 'done' comes before completed
            Thread.Sleep(2000); // blocks 'done' until after completed
            Log("completed");
        }

        [Test]
        public async Task Test2()
        {
            Log("start");
            var taskCompletionSource = new TaskCompletionSource<int>();
            var task = Task.Run(async () =>
            {
                Log("wait");
                await Task.Delay(2000).ConfigureAwait(false);
                Log("complete");
                taskCompletionSource.SetResult(42);
                Log("done");
            });
            Log("wait completion");

            await taskCompletionSource.Task;
            //await Task.Delay(2000); // 'done' comes before completed
            Thread.Sleep(2000); // blocks 'done' until after completed
            Log("completed");
        }

        [Test]
        public async Task Test3()
        {
            Log("start");
            var taskCompletionSource = new TaskCompletionSource<int>();
            var task = Task.Run(async () =>
            {
                Log("wait");
                await Task.Delay(2000).ConfigureAwait(false);
                Log("complete");
                taskCompletionSource.SetResult(42);
                Log("done");
            });
            Log("wait completion");

            await taskCompletionSource.Task.ConfigureAwait(false);
            //await Task.Delay(2000); // 'done' comes before completed
            Thread.Sleep(2000); // blocks 'done' until after completed
            Log("completed");
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

        [Test]
        [Timeout(20_000)]
        public async Task Timeout1Test()
        {
            var remainingMilliseconds = 10_000;

            var task = Task.Delay(2_000).ContinueWith(_ => 2);

            var i = await task.WithTimeout(remainingMilliseconds);

            await Task.Delay(2_000);

            Assert.AreEqual(2, i);
        }

        [Test]
        [Timeout(20_000)]
        public async Task Timeout2Test()
        {
            var remainingMilliseconds = 1_000;
            var taskexec = false;

            var task = Task.Delay(2_000).ContinueWith(t =>
            {
                if (!t.IsCompleted || t.IsCanceled) return 0;
                taskexec = true;
                return 2;
            });

            Assert.ThrowsAsync<TimeoutException>(async () => await task.WithTimeout(remainingMilliseconds));

            await Task.Delay(2_000);
            Assert.IsTrue(taskexec);
        }

        [Test]
        [Timeout(20_000)]
        public async Task Timeout3Test()
        {
            var remainingMilliseconds = 1_000;
            var taskexec = false;

            var taskCancel = new CancellationTokenSource();
            var task = Task.Delay(2_000, taskCancel.Token).ContinueWith(t =>
            {
                if (!t.IsCompleted || t.IsCanceled) return 0;
                taskexec = true;
                return 2;
            });

            Assert.ThrowsAsync<TimeoutException>(async () => await task.WithTimeout(remainingMilliseconds, taskCancel));

            await Task.Delay(2_000);
            Assert.IsFalse(taskexec);
        }

        [Test]
        [Timeout(20_000)]
        public async Task Timeout4Test()
        {
            var remainingMilliseconds = 1_000;
            var taskexec = false;

            var task = Task.Delay(100).ContinueWith(t =>
            {
                throw new Exception("bang");
                return 2;
            });

            Assert.ThrowsAsync<Exception>(async () => await task.WithTimeout(remainingMilliseconds));

            await Task.Delay(2_000);
            Assert.IsFalse(taskexec);
        }

        [Test]
        [Timeout(20_000)]
        public async Task Timeout5Test()
        {
            var remainingMilliseconds = 1_000;
            var taskexec = false;
            var semaphore = new SemaphoreSlim(0);

            async Task RunTask(CancellationToken token)
            {
                await Task.Delay(100);
                try
                {
                    await semaphore.WaitAsync(token);
                    Console.WriteLine("1");
                }
                catch (Exception e)
                {
                    Console.WriteLine("2: " + e);
                    throw;
                }
            }

            var taskCancel = new CancellationTokenSource();
            var task = RunTask(taskCancel.Token);

            Assert.ThrowsAsync<Exception>(async () => await task.WithTimeout(remainingMilliseconds, taskCancel));

            await Task.Delay(2_000);
            Assert.IsFalse(taskexec);
        }

        private void Log(string msg)
        {
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId:00}] " + msg);
        }

        private async Task DoSomething()
        {
            await Task.CompletedTask;
        }
    }

    public static class Extensions2
    {
        public static async Task WithTimeout(this Task task, int timeoutMilliseconds, CancellationTokenSource taskCancel = null)
        {
            using (var timeoutCancel = new CancellationTokenSource())
            {
                var timeoutTask = Task.Delay(timeoutMilliseconds, timeoutCancel.Token);

                await Task.WhenAny(task, timeoutTask);

                if (task.IsCompleted)
                {
                    // timeoutTask is never awaited,
                    // results & exceptions will be ignored
                    timeoutCancel.Cancel();
                }

                // cancel the task
                taskCancel?.Cancel();

                throw new TimeoutException();
            }
        }

        public static async Task<T> WithTimeout<T>(this Task<T> task, int timeoutMilliseconds, CancellationTokenSource taskCancel = null)
        {
            using (var timeoutCancel = new CancellationTokenSource())
            {
                var timeoutTask = Task.Delay(timeoutMilliseconds, timeoutCancel.Token);

                await Task.WhenAny(task, timeoutTask);

                if (task.IsCompleted)
                {
                    // timeoutTask is never awaited,
                    // results & exceptions will be ignored
                    timeoutCancel.Cancel();

                    // https://stackoverflow.com/questions/24623120/await-on-a-completed-task-same-as-task-result
                    // return task.Result;
                    return await task;
                }

                // cancel the task
                taskCancel?.Cancel();

                throw new TimeoutException();
            }
        }
    }
}
