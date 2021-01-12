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

using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    internal abstract class EventSubscriptionBase : IAsyncDisposable
    {
        private readonly SemaphoreSlim _busy = new SemaphoreSlim(1);
        private int _disposed;
        private int _subscriptionsCount;
        private Guid _subscriptionId;

        protected EventSubscriptionBase(ClusterState clusterState, ClusterEvents clusterEvents)
        {
            LoggerFactory = clusterState.LoggerFactory;
            ClusterEvents = clusterEvents;
        }

        protected ClusterEvents ClusterEvents { get; }

        protected ILoggerFactory LoggerFactory { get; }

        protected abstract ClusterSubscription CreateSubscription();

        public async Task AddSubscription()
        {
            if (_disposed == 1) throw new ObjectDisposedException(nameof(EventSubscriptionBase));

            // accepted race condition, _busy can be disposed here and will throw an ObjectDisposedException

            await _busy.WaitAsync().CfAwait();
            try
            {
                _subscriptionsCount++;
                if (_subscriptionsCount > 1) return;

                var subscription = CreateSubscription();
                await ClusterEvents.InstallSubscriptionAsync(subscription).CfAwait();
                _subscriptionId = subscription.Id;
            }
            finally
            {
                _busy.Release();
            }
        }

        public async ValueTask<bool> RemoveSubscription()
        {
            if (_disposed == 1) throw new ObjectDisposedException(nameof(EventSubscriptionBase));

            // accepted race condition, _busy can be disposed here and will throw an ObjectDisposedException

            await _busy.WaitAsync().CfAwait();
            try
            {
                if (_subscriptionsCount == 0) return true; // TODO: should we throw?
                if (_subscriptionsCount > 1) return true;

                var removed = await ClusterEvents.RemoveSubscriptionAsync(_subscriptionId).CfAwait();
                if (removed) _subscriptionsCount--;
                return removed;
            }
            finally
            {
                _busy.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            await _busy.WaitAsync().CfAwait();
            try
            {
                if (_subscriptionsCount == 0) return;

                // remove, ignore result
                await ClusterEvents.RemoveSubscriptionAsync(_subscriptionId).CfAwait();
            }
            finally
            {
                _busy.Release();
            }

            _busy.Dispose();

            // this should not be a warning
            // https://github.com/dotnet/roslyn-analyzers/issues/3909
            // https://github.com/dotnet/roslyn-analyzers/issues/3675
            // https://github.com/dotnet/roslyn-analyzers/pull/3679
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            GC.SuppressFinalize(this);
#pragma warning restore CA1816
        }
    }
}
