using System;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="IDisposable"/> interface.
    /// </summary>
    public static class DisposableExtensions
    {
        /// <summary>
        /// Tries to dispose an <see cref="IDisposable"/> and swallow exceptions.
        /// </summary>
        /// <param name="disposable">The disposable.</param>
        public static void TryDispose(this IDisposable disposable)
        {
            if (disposable == null) return;

            // TODO: evil, don't swallow exceptions
            try
            {
                disposable.Dispose();
            }
            catch { /* nothing */ }
        }
    }
}
