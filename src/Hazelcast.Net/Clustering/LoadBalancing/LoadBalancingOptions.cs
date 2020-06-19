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

using Hazelcast.Configuration.Binding;
using Hazelcast.Core;

namespace Hazelcast.Clustering.LoadBalancing
{
    /// <summary>
    /// Represents the load balancing options.
    /// </summary>
    public class LoadBalancingOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancingOptions"/> class.
        /// </summary>
        public LoadBalancingOptions()
        {
            LoadBalancer = new SingletonServiceFactory<ILoadBalancer>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancingOptions"/> class.
        /// </summary>
        private LoadBalancingOptions(LoadBalancingOptions other)
        {
            LoadBalancer = other.LoadBalancer.Clone();
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
            set => LoadBalancer.Creator = () => ServiceFactory.CreateInstance<ILoadBalancer>(value.TypeName, value.Args);
        }

        /// <summary>
        /// Clone the options.
        /// </summary>
        internal LoadBalancingOptions Clone() => new LoadBalancingOptions(this);
    }
}
