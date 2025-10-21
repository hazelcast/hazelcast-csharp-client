using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Polyfills
{
    static class CancellationTokenSourceExtensions
    {
        public static Task TryCancelAsync(this CancellationTokenSource cancel)
        {
#if NET8_0_OR_GREATER
                return cancel.CancelAsync();
#else
                cancel.Cancel();
                return Task.CompletedTask;
#endif
        }
    }
}
