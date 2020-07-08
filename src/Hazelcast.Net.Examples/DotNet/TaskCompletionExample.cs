using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Examples.DotNet
{
    // ReSharper disable once UnusedMember.Global
    public class TaskCompletionExample : ExampleBase
    {
        private static int _count;

        public static void Run(string[] args)
        {
#if DEBUG
            // cannot work in DEBUG mode because the reference to the task stays around
            // too long, it's not GC / finalized in time for the exception to be reported
            Console.WriteLine("When built with Configuration=Debug, this example is expected to fail.");
            Console.WriteLine();

            // note that we could get it to work by creating the task in a method, see
            // ObservedExceptionTests, but the purpose here is to demo the effect of DEBUG
#endif

            RunUnobserved(args);
            Console.WriteLine();

#if DEBUG
            // GC here because the unobserved exception is now in the queue, because the
            // scope of the task variable has been exited so even with Configuration=Debug
            // the task can be finalized - and we don't want it to pollute the next run
            GC.Collect();
            GC.WaitForPendingFinalizers();
#endif

            RunObserved(args);
        }

        private static void RunUnobserved(string[] args)
        {
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            _count = 0;

            try
            {
                var task = Task.Run(() => throw new Exception("unobserved bang!"));

                // wait for the task to complete
                while (!task.IsCompleted) Thread.Sleep(100);

                // supposedly, that would be a way to wait that does not observe the exception like 'await' would
                // but in reality in .NET Core 3.x at least, it seems to observe the exception
                //((IAsyncResult) task).AsyncWaitHandle.WaitOne();

                // set to null so it can be GC / finalized
                task = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            finally
            {
                TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
            }

            Console.WriteLine($"Count: {_count}");
            Console.WriteLine(_count == 1 ? "Success!" : "Failed, expected 1.");
#if DEBUG
            Console.WriteLine("Failure is normal when Configuration=Debug.");
#endif
        }

        private static void RunObserved(string[] args)
        {
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            _count = 0;

            try
            {
                var task = Task.Run(() => throw new Exception("observed bang!"))

                    // observe
                    .ContinueWith(t =>
                    {
                        _ = t.Exception;
                    }, TaskScheduler.Current);

                // wait for the task to complete
                while (!task.IsCompleted) Thread.Sleep(100);

                // supposedly, that would be a way to wait that does not observe the exception like 'await' would
                // but in reality in .NET Core 3.x at least, it seems to observe the exception
                //((IAsyncResult) task).AsyncWaitHandle.WaitOne();

                // set to null so it can be GC / finalized
                task = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            finally
            {
                TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
            }

            Console.WriteLine($"Count: {_count}");
            Console.WriteLine(_count == 0 ? "Success!" : "Failed, expected 0.");
        }

        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            Console.WriteLine("Unobserved exception: " + args.Exception.Flatten().InnerException.Message);
            _count++;
            args.SetObserved();
        }
    }
}
