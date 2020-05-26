using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents a lock acquisition.
    /// </summary>
    public class LockAcquisition : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockAcquisition"/> class.
        /// </summary>
        /// <param name="semaphore">A semaphore.</param>
        private LockAcquisition(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        /// <summary>
        /// Acquires a lock.
        /// </summary>
        /// <param name="semaphore">The semaphore.</param>
        /// <returns>A <see cref="LockAcquisition"/> instance that needs to be disposed to release the lock.</returns>
        public static
#if !OPTIMIZE_ASYNC
            async
#endif
            ValueTask<LockAcquisition> WaitAsync(SemaphoreSlim semaphore)
        {
            var task = new LockAcquisition(semaphore).WaitAsync();

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <summary>
        /// Tries to acquire a lock immediately.
        /// </summary>
        /// <param name="semaphore">The semaphore.</param>
        /// <returns>A <see cref="LockAcquisition"/> instance that needs to be disposed to release the lock.</returns>
        public static
#if !OPTIMIZE_ASYNC
            async
#endif
            ValueTask<LockAcquisition> TryLockAsync(SemaphoreSlim semaphore)
        {
            var task = new LockAcquisition(semaphore).TryLockAsync();

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <summary>
        /// Whether the lock was acquired.
        /// </summary>
        public bool Acquired { get; private set; }

        /// <summary>
        /// Asynchronously waits to enter the lock.
        /// </summary>
        /// <returns>A <see cref="LockAcquisition"/> instance that must be disposed to exit the lock.</returns>
        private async ValueTask<LockAcquisition> WaitAsync()
        {
            await _semaphore.WaitAsync();
            Acquired = true;
            return this;
        }

        /// <summary>
        /// Asynchronous tries to immediately enter the lock.
        /// </summary>
        /// <returns>A <see cref="LockAcquisition"/> instance that must be disposed to exit the lock.</returns>
        /// <remarks>
        /// <para>This method does not wait to enter the lock. Either it can enter immediately,
        /// or it does not enter. If the lock is not entered, nothing will happen when the
        /// acquisition is disposed.</para>
        /// </remarks>
        private async ValueTask<LockAcquisition> TryLockAsync()
        {
            Acquired = await _semaphore.WaitAsync(0);
            return this;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            if (Acquired) _semaphore.Release();
        }

        /// <summary>
        /// Implicitly convert a <see cref="LockAcquisition"/> into a boolean.
        /// </summary>
        /// <param name="acquisition">The acquisition.</param>
        public static implicit operator bool(LockAcquisition acquisition)
            => acquisition.Acquired;
    }
}
