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
        public static async Task ObserveException(this Task task)
        {
            if (task == null) return;

            try
            {
                await task.ConfigureAwait(false);
            }
            catch
            {
                // observe the exception
            }
        }

        /// <summary>
        /// Observes the exception of a faulted task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>A task with an observed exception.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<T> ObserveException<T>(this Task<T> task)
        {
            if (task == null) return default;

            try
            {
                return await task.ConfigureAwait(false);
            }
            catch
            {
                // observe the exception
                return default;
            }
        }

        /// <summary>
        /// Observes a canceled task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>A task that will complete when the task completes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task ObserveCanceled(this Task task)
        {
            if (task == null) return;

            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // observe the exception
            }
        }
    }
}
