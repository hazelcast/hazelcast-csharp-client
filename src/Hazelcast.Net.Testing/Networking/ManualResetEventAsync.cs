// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Testing.Networking
{
    /// <summary>
    /// Represents an asynchronous manual reset event.
    /// </summary>
    public class ManualResetEventAsync
    {
        // inspiration:
        // https://badflyer.com/asyncmanualresetevent/
        // https://stackoverflow.com/questions/18756354/wrapping-manualresetevent-as-awaitable-task

        private volatile TaskCompletionSource<object> _completionSource = new TaskCompletionSource<object>();

        public ManualResetEventAsync(bool isSet)
        {
            if (isSet) _completionSource.TrySetResult(null);
        }

        public bool IsSet => _completionSource.Task.IsCompleted;

        public void Set()
        {
            _completionSource.TrySetResult(null);
        }

        public void Reset()
        {
            var completionSource = _completionSource;

            if (!completionSource.Task.IsCompleted) return;

            Interlocked.CompareExchange(ref _completionSource, new TaskCompletionSource<object>(), completionSource);
        }

        public async Task WaitAsync(CancellationToken cancellationToken)
        {
            var cancellationSource = new TaskCompletionSource<object>();
            using var reg = cancellationToken.Register(() => cancellationSource.TrySetResult(null));

            var task = await Task.WhenAny(_completionSource.Task, cancellationSource.Task).CfAwait();

            if (task != cancellationSource.Task) return;

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
