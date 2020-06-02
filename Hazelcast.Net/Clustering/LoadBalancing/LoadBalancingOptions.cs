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
using Hazelcast.Core;
using Hazelcast.Exceptions;

namespace Hazelcast.Clustering.LoadBalancing
{
    /// <summary>
    /// Represents the load balancing options.
    /// </summary>
    public class LoadBalancingOptions
    {
        private string _loadBalancerType;

        public LoadBalancingOptions()
        {
            LoadBalancer = new ServiceFactory<ILoadBalancer>();
            LoadBalancerArgs = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the service factory for <see cref="ILoadBalancer"/>.
        /// </summary>
        public ServiceFactory<ILoadBalancer> LoadBalancer { get; private set; }

        /// <summary>
        /// Gets or sets the type of the load balancer.
        /// </summary>
        /// <remarks>
        /// <para>Returns the correct value only if it has been set via the same property. If the
        /// load balancer has been configured via code and the <see cref="LoadBalancer"/>
        /// property, the value returned by this property is unspecified.</para>
        /// </remarks>
        public string LoadBalancerType
        {
            get => _loadBalancerType;

            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(value));

                _loadBalancerType = value;

                LoadBalancer.Creator = () => Services.CreateInstance<ILoadBalancer>(value, this);
            }
        }

        /// <summary>
        /// Gets the arguments for the load balancer.
        /// </summary>
        /// <remarks>
        /// <para>Arguments are used when creating a load balancer from its type as set
        /// via the <see cref="LoadBalancerType"/> property. They are ignored if the
        /// credentials factory has been configured via code and the <see cref="LoadBalancer"/>
        /// property.</para>
        /// </remarks>
        public Dictionary<string, object> LoadBalancerArgs { get; private set; }

        /// <summary>
        /// Clone the options.
        /// </summary>
        public LoadBalancingOptions Clone()
        {
            return new LoadBalancingOptions
            {
                LoadBalancer = LoadBalancer.Clone(),
                _loadBalancerType = _loadBalancerType,
                LoadBalancerArgs = new Dictionary<string, object>(LoadBalancerArgs)
            };
        }
    }
}
