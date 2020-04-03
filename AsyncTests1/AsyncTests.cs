using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncTests1
{
    [TestFixture]
    public class AsyncTests
    {
        // https://stackoverflow.com/questions/19481964/calling-taskcompletionsource-setresult-in-a-non-blocking-manner
        // http://blog.stephencleary.com/2012/12/dont-block-in-asynchronous-code.html

        // taskCompletionSource.SetResult() scheduled with .ExecuteSynchronously = duh

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
