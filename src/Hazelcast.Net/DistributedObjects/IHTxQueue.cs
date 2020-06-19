// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    /// <summary>Transactional implementation of Queue</summary>
    public interface IHTxQueue<TItem> : ITransactionalObject
    {
        Task<bool> TryEnqueueAsync(TItem item);
        Task<bool> TryEnqueueAsync(TItem item, CancellationToken cancellationToken);
        Task<bool> TryEnqueueAsync(TItem item, TimeSpan timeToWait);
        Task<bool> TryEnqueueAsync(TItem item, TimeSpan timeToWait, CancellationToken cancellationToken);

        Task<TItem> PeekAsync(TimeSpan timeout = default);
        Task<TItem> PeekAsync(CancellationToken cancellationToken);

        Task<TItem> TryPeekAsync(TimeSpan timeToWait, TimeSpan timeout = default);
        Task<TItem> TryPeekAsync(TimeSpan timeToWait, CancellationToken cancellationToken);

        Task<TItem> TryDequeueAsync();
        Task<TItem> TryDequeueAsync(CancellationToken cancellationToken);
        Task<TItem> TryDequeueAsync(TimeSpan timeToWait);
        Task<TItem> TryDequeueAsync(TimeSpan timeToWait, CancellationToken cancellationToken);

        Task<int> CountAsync(TimeSpan timeout = default);
        Task<int> CountAsync(CancellationToken cancellationToken);

        Task<TItem> DequeueAsync(TimeSpan timeout = default);
        Task<TItem> DequeueAsync(CancellationToken cancellationToken);
    }
}
