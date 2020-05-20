using System;
using System.Threading;

namespace Hazelcast.Core
{
    internal class SemaphoreDisposableReleaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private int _disposed;

        public SemaphoreDisposableReleaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            _semaphore.Release();
        }
    }
}
