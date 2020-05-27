using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    public static class ObserveTaskExtensions
    {
        private static void ObserveTaskException(Task Task)
        {
            _ = Task.Exception;
        }

        public static Task ObserveException(this Task task)
        {
            return task.ContinueWith(t => { ObserveTaskException(t); }, TaskContinuationOptions.NotOnRanToCompletion);
            //return task.ContinueWith(t => { ObserveTaskException(t); });
        }

        //public static Task<T> ObserveException<T>(this Task<T> task)
        //{
        //    return task.ContinueWith(t => { }, TaskContinuationOptions.OnlyOnCanceled|TaskContinuationOptions.NotOnFaulted);
        //}
    }

    public class ObservedExceptionTests
    {
        [SetUp]
        public void Setup()
        {
            _log = new StringBuilder();
        }

        private void Observe()
        {
            // why doesn't this fire?
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                _log.AppendLine("unobserved exception! " + args.Exception);
            };
        }

        [Test]
        public async Task Test1()
        {
            var source = new CancellationTokenSource();
            _ = DoAsync(source.Token);
            //task.CAF().GetAwaiter().OnCompleted(OnCompleted);

            await Task.Delay(1000, CancellationToken.None).CAF();

            source.Cancel();

            // observes the exception
            //await task.CAF();

            //task = task.ContinueWith(x =>
            //{
            //    if (x.IsFaulted) Console.WriteLine("Observed: " + x.Exception);
            //});
            //await task; // exception is gone, has been observed


            GC.Collect();
            GC.WaitForPendingFinalizers();

            //await Task.Delay(1000, CancellationToken.None).CAF();
        }

        private StringBuilder _log;

        [Test]
        public async Task Test2()
        {
            Observe();

            _log.AppendLine("run");
            var task = Throw();

            task.CAF()
                .GetAwaiter()
                .OnCompleted(OnCompleted);

            // configure await vs continue with?
            // this DOES observe the exception and we are happy
            var observing = task.ObserveException();

            await Task.Delay(4000).CAF();

            // these 3 lines are required in order to observe!
            task = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            await Task.Delay(4000).CAF();

            _log.AppendLine("end");
            Console.WriteLine(_log.ToString());
        }

        public async Task Throw()
        {
            await Task.Delay(1000).CAF();
            _log.AppendLine("throw");
            throw new Exception("bang");
        }

        public void OnCompleted()
        {
            _log.AppendLine("completed");
        }

        public async Task DoAsync()
        {
            await Task.Delay(100).CAF();
            throw new Exception("bang");
        }

        public async Task DoAsync(CancellationToken token)
        {
            Console.WriteLine("doAsync start");
            try
            {
                await Task.Delay(10 * 1000, token).CAF();
            }
            catch
            {
                Console.WriteLine("exception!");
                throw;
            }
            Console.WriteLine("doAsync end");
        }

        public void Wip()
        {
            var t = IntAsync().ContinueWith(x => { _ = x.Exception;
                return x.Result;
            });
        }

        public Task<int> IntAsync()
        {
            return Task.FromResult(3);
        }
    }
}