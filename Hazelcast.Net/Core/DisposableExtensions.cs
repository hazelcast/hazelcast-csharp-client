using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="IDisposable"/> interface.
    /// </summary>
    public static class DisposableExtensions
    {
        /// <summary>
        /// Tries to dispose an <see cref="IDisposable"/> without throwing.
        /// </summary>
        /// <param name="disposable">The disposable.</param>
        /// <param name="logger">An optional logger.</param>
        public static void TryDispose(this IDisposable disposable, ILogger logger = null)
        {
            if (disposable == null) return;

            try
            {
                disposable.Dispose();
            }
            catch (Exception e)
            {
                logger.LogWarning(e, $"Caught an exception while disposing {disposable.GetType()}.");
            }
        }

        /// <summary>
        /// Tries to dispose an <see cref="IAsyncDisposable"/> without throwing.
        /// </summary>
        /// <param name="disposable">The disposable.</param>
        /// <param name="logger">An optional logger.</param>
        /// <returns>A task that completes when the disposable has been disposed.</returns>
        public static async ValueTask TryDisposeAsync(this IAsyncDisposable disposable, ILogger logger = null)
        {
            if (disposable == null) return;

            try
            {
                await disposable.DisposeAsync().CAF();
            }
            catch (Exception e)
            {
                logger.LogWarning(e, $"Caught an exception while disposing {disposable.GetType()}.");
            }
        }
    }
}
