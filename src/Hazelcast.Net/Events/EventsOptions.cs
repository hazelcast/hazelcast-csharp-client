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

namespace Hazelcast.Events
{
    /// <summary>
    /// Represents the events options.
    /// </summary>
    public class EventsOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventsOptions"/> class.
        /// </summary>
        public EventsOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsOptions"/> class.
        /// </summary>
        private EventsOptions(EventsOptions other)
        {
            SubscriptionCollectDelay = other.SubscriptionCollectDelay;
            SubscriptionCollectPeriod = other.SubscriptionCollectPeriod;
            SubscriptionCollectTimeout = other.SubscriptionCollectTimeout;
        }

        /// <summary>
        /// Gets or sets the delay before collecting subscriptions starts.
        /// </summary>
        public TimeSpan SubscriptionCollectDelay { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets the period of the subscription collection.
        /// </summary>
        public TimeSpan SubscriptionCollectPeriod { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets the subscription collection timeout, after which a subscription is considered dead and removed.
        /// </summary>
        public TimeSpan SubscriptionCollectTimeout { get; set; } = TimeSpan.FromMinutes(4);
    }
}
