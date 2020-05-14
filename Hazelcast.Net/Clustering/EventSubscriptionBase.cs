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
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    internal abstract class EventSubscriptionBase
    {
        private int _subscriptionsCount;
        private Guid _subscriptionId;

        protected EventSubscriptionBase(Cluster cluster, ILoggerFactory loggerFactory)
        {
            Cluster = cluster;
            LoggerFactory = loggerFactory;
        }

        protected Cluster Cluster { get; }

        protected ILoggerFactory LoggerFactory { get; }

        protected abstract ClusterSubscription CreateSubscription();

        public async Task AddSubscription()
        {
            // add a subscription, increment returns the incremented value
            // so it's 1 for the first subscription - which requires an actual
            // cluster subscription
            if (Interlocked.Increment(ref _subscriptionsCount) > 1)
                return;

            var subscription = CreateSubscription();

            await Cluster.InstallSubscriptionAsync(subscription);

            _subscriptionId = subscription.Id;
        }

        public async Task RemoveSubscription()
        {
            // remove a subscription, decrement returns the decremented value
            // so it's 0 if we don't have subscriptions anymore and can
            // unsubscribe the cluster
            if (Interlocked.Decrement(ref _subscriptionsCount) > 0)
                return;

            await Cluster.RemoveSubscriptionAsync(_subscriptionId);
            _subscriptionId = default;
        }
    }
}