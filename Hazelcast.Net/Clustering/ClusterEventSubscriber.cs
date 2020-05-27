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
using Hazelcast.Core;
using Hazelcast.Exceptions;

namespace Hazelcast.Clustering
{
    internal class ClusterEventSubscriber : IClusterEventSubscriber
    {
        private readonly Func<Cluster, Task> _subscribeAsync;
        private readonly Type _type;
        private readonly string _typename;
        private readonly IClusterEventSubscriber _subscriber;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterEventSubscriber"/> class.
        /// </summary>
        /// <param name="subscribeAsync">A subscribe method.</param>
        public ClusterEventSubscriber(Func<Cluster, Task> subscribeAsync)
        {
            _subscribeAsync = subscribeAsync ?? throw new ArgumentNullException(nameof(subscribeAsync));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterEventSubscriber"/> class.
        /// </summary>
        /// <param name="type">A subscriber class type.</param>
        public ClusterEventSubscriber(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterEventSubscriber"/> class.
        /// </summary>
        /// <param name="typename">A subscriber class type name.</param>
        public ClusterEventSubscriber(string typename)
        {
            if (string.IsNullOrWhiteSpace(typename)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(typename));
            _typename = typename;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterEventSubscriber"/> class.
        /// </summary>
        /// <param name="subscriber">A subscriber class instance.</param>
        public ClusterEventSubscriber(IClusterEventSubscriber subscriber)
        {
            _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));
        }

        /// <inheritdoc />
        public async Task SubscribeAsync(Cluster cluster, CancellationToken cancellationToken)
        {
            if (_subscribeAsync != null)
            {
                await _subscribeAsync(cluster).CAF();
            }
            else
            {
                var subscriber = _subscriber ?? (_type == null
                    ? Services.CreateInstance<IClusterEventSubscriber>(_typename)
                    : Services.CreateInstance<IClusterEventSubscriber>(_type));

                await subscriber.SubscribeAsync(cluster, cancellationToken).CAF();
            }
        }
    }
}