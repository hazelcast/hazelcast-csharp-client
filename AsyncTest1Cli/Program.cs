using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTest1Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // why doesn't this fire?
            TaskScheduler.UnobservedTaskException += (sender, args) => { Console.WriteLine("unobserved exception! " + args.Exception); };

            var source = new CancellationTokenSource();
            var task = DoAsync(source.Token);
            //task.ConfigureAwait(false).GetAwaiter().OnCompleted(OnCompleted);

            await Task.Delay(1000, CancellationToken.None);

            source.Cancel();

            // observes the exception
            //await task;

            //task = task.ContinueWith(x =>
            //{
            //    if (x.IsFaulted) Console.WriteLine("Observed: " + x.Exception);
            //});
            //await task; // exception is gone, has been observed

            task = null;
            source = null;

            Console.WriteLine("collect");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("/collect");

            Console.ReadLine();

            //await Task.Delay(1000, CancellationToken.None);
        }

        public static void OnCompleted()
        {
            Console.WriteLine("completed");
        }

        public static async Task DoAsync()
        {
            await Task.Delay(100);
            throw new Exception("bang");
        }

        public static async Task DoAsync(CancellationToken token)
        {
            Console.WriteLine("doAsync start");
            try
            {
                await Task.Delay(10 * 1000, token);
            }
            catch
            {
                Console.WriteLine("exception!");
                throw;
            }

            Console.WriteLine("doAsync end");
        }
    }
}