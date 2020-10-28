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

using Hazelcast.Configuration;
using Hazelcast.Configuration.Binding;
using Hazelcast.Core;

namespace Hazelcast.Clustering.LoadBalancing
{
    /// <summary>
    /// Represents the load balancing options.
    /// </summary>
    public class LoadBalancingOptions
    {
        private LoadBalancingMode _mode;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancingOptions"/> class.
        /// </summary>
        public LoadBalancingOptions()
        {
            LoadBalancer = new SingletonServiceFactory<ILoadBalancer> { Creator = () => new RandomLoadBalancer() };
            _mode = LoadBalancingMode.Random;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancingOptions"/> class.
        /// </summary>
        private LoadBalancingOptions(LoadBalancingOptions other)
        {
            LoadBalancer = other.LoadBalancer.Clone();
            _mode = other._mode;
        }

        /// <summary>
        /// Gets or sets the load balancing mode.
        /// </summary>
        public LoadBalancingMode Mode
        {
            get => _mode;
            set
            {
                // this would be nicer with a switch expression but dotCover (as of 2020.2.3) does no cover them
                switch (_mode = value)
                {
                    case LoadBalancingMode.Random:
                        LoadBalancer.Creator = () => new RandomLoadBalancer();
                        break;

                    case LoadBalancingMode.RoundRobin:
                        LoadBalancer.Creator = () => new RoundRobinLoadBalancer();
                        break;

                    case LoadBalancingMode.Custom:
                        LoadBalancer.Creator = null;
                        break;

                    default:
                        throw new ConfigurationException($"Invalid load balancing mode \"{value}\".");
                }
            }
        }

        /// <summary>
        /// Gets the service factory for <see cref="ILoadBalancer"/>.
        /// </summary>
        [BinderIgnore]
        public SingletonServiceFactory<ILoadBalancer> LoadBalancer { get; }

        [BinderName("loadBalancer")]
        [BinderIgnore(false)]
#pragma warning disable IDE0051 // Remove unused private members
        // ReSharper disable once UnusedMember.Local
        private InjectionOptions LoadBalancerBinder
#pragma warning restore IDE0051 // Remove unused private members
        {
            get => default;
            set
            {
                _mode = LoadBalancingMode.Custom;
                LoadBalancer.Creator = () => ServiceFactory.CreateInstance<ILoadBalancer>(value.TypeName, value.Args);
            }
        }

        /// <summary>
        /// Clone the options.
        /// </summary>
        internal LoadBalancingOptions Clone() => new LoadBalancingOptions(this);
    }
}
