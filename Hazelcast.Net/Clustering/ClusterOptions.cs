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
using System.Collections.Generic;
using System.Xml;
using Hazelcast.Configuration;
using Hazelcast.Configuration.Binding;
using Hazelcast.Core;

namespace Hazelcast.Clustering
{
    public class ClusterOptions
    {
        public ClusterOptions()
        {
            EventSubscribers = new List<IClusterEventSubscriber>();
            EventSubscribersBinder = new CollectionBinder<string>(x
                => EventSubscribers.Add(new ClusterEventSubscriber(x)));
        }

        /// <summary>
        /// Gets the default cluster name.
        /// </summary>
        public const string DefaultClusterName = "dev";

        public string Name { get; set; } = DefaultClusterName;

        public bool ShuffleMemberList { get; set; } = true;

        /// <summary>
        /// Gets the cluster event subscribers.
        /// </summary>
        [BinderIgnore]
        public List<IClusterEventSubscriber> EventSubscribers { get; private set; }

        // used for configuration binding
        [BinderName("eventSubscribers")]
        [BinderIgnore(false)]
#pragma warning disable IDE0052 // Remove unread private members
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private ICollection<string> EventSubscribersBinder { get; }
#pragma warning restore IDE0052 // Remove unread private members

        public ClusterOptions AddEventSubscriber(Action<ClusterEventHandlers> on)
        {
            EventSubscribers.Add(new ClusterEventSubscriber((cluster, cancellationToken)
                => cluster.SubscribeAsync(on, cancellationToken)));
            return this;
        }

        public ClusterOptions AddEventSubscriber(IClusterEventSubscriber subscriber)
        {
            EventSubscribers.Add(new ClusterEventSubscriber(subscriber));
            return this;
        }

        public ClusterOptions AddEventSubscriber<T>()
            where T : IClusterEventSubscriber
        {
            EventSubscribers.Add(new ClusterEventSubscriber(typeof(T)));
            return this;
        }

        public ClusterOptions AddEventSubscriber(Type type)
        {
            EventSubscribers.Add(new ClusterEventSubscriber(type));
            return this;
        }

        public ClusterOptions AddEventSubscriber(string typename)
        {
            EventSubscribers.Add(new ClusterEventSubscriber(typename));
            return this;
        }

        /// <summary>
        /// Clones the options.
        /// </summary>
        public ClusterOptions Clone()
        {
            return new ClusterOptions
            {
                Name = Name,
                ShuffleMemberList = ShuffleMemberList,
                EventSubscribers = new List<IClusterEventSubscriber>(EventSubscribers)
            };
        }
    }
}
