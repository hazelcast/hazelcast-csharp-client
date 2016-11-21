using System.Threading.Tasks;

namespace Hazelcast.Util
{
    internal static class TaskExtension
    {
        // The unobserved task handler extention. simply  observes the exception and do nothing.
        public static Task IgnoreExceptions(this Task task)
        {
            task.ContinueWith(c => { var ignored = c.Exception; },
                TaskContinuationOptions.OnlyOnFaulted |
                TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }
    }
}