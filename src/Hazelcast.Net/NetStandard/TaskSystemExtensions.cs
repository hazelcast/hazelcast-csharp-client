using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System
{
    internal static class TaskSystemExtensions
    {
        /// <summary>
        /// Observes the exception of a faulted task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>A task with an observed exception.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task ObserveException(this Task task)
            => task.ContinueWith(t =>
                {
                    if (!t.IsFaulted) return t;
                    _ = t.Exception;
                    return Task.CompletedTask;
                },
                default,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current).Unwrap();

        /// <summary>
        /// Observes the exception of a faulted task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>A task with an observed exception.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<T> ObserveException<T>(this Task<T> task)
            => task.ContinueWith(t =>
                {
                    if (!t.IsFaulted) return t;
                    _ = t.Exception;
                    return Task.FromResult(default(T)!);
                },
                default,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current).Unwrap();
    }
}
