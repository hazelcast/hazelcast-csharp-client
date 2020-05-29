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
using Hazelcast.Core;

namespace Hazelcast.Clustering
{
    public class ClusterConfiguration
    {
        /// <summary>
        /// Gets the cluster event subscribers.
        /// </summary>
        public List<IClusterEventSubscriber> EventSubscribers { get; } = new List<IClusterEventSubscriber>();

        public ClusterConfiguration AddEventSubscriber(Action<ClusterEventHandlers> on)
        {
            EventSubscribers.Add(new ClusterEventSubscriber((cluster, cancellationToken)
                => cluster.SubscribeAsync(on, cancellationToken)));
            return this;
        }

        public ClusterConfiguration AddEventSubscriber(IClusterEventSubscriber subscriber)
        {
            EventSubscribers.Add(new ClusterEventSubscriber(subscriber));
            return this;
        }

        public ClusterConfiguration AddEventSubscriber<T>()
            where T : IClusterEventSubscriber
        {
            EventSubscribers.Add(new ClusterEventSubscriber(typeof(T)));
            return this;
        }

        public ClusterConfiguration AddEventSubscriber(Type type)
        {
            EventSubscribers.Add(new ClusterEventSubscriber(type));
            return this;
        }

        public ClusterConfiguration AddEventSubscriber(string typename)
        {
            EventSubscribers.Add(new ClusterEventSubscriber(typename));
            return this;
        }

        public static ClusterConfiguration Parse(XmlNode node)
        {
            var configuration = new ClusterConfiguration();

            foreach (XmlNode child in node.ChildNodes)
            {
                if ("listener".Equals(child.GetCleanName()))
                {
                    var className = child.GetTextContent();
                    configuration.AddEventSubscriber(className);
                }
            }

            return configuration;
        }
    }
}
