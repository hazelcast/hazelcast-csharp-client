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

        private void Log(string msg)
        {
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId:00}] " + msg);
        }

        private async Task DoSomething()
        {
            await Task.CompletedTask;
        }
    }
}
