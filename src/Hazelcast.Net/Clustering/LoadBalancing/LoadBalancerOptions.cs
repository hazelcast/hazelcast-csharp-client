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

using Hazelcast.Configuration.Binding;
using Hazelcast.Core;

namespace Hazelcast.Clustering.LoadBalancing
{
    /// <summary>
    /// Implements instance creation logic of <see cref="LoadBalancer""> singleton service as property.
    /// </summary>
    public abstract class LoadBalancerOptions
    {
        /// <summary>
        /// Gets the <see cref="SingletonServiceFactory{TService}"/> for the <see cref="ILoadBalancer"/>.
        /// </summary>
        /// <remarks>
        /// <para>When set in the configuration file, it is defined as an injected type, for instance:
        /// <code>
        /// "loadBalancer":
        /// {
        ///   "typeName": "My.LoadBalancer",
        ///   "args":
        ///   {
        ///     "foo": 42
        ///   }
        /// }
        /// </code>
        /// where <c>typeName</c> is the name of the type, and <c>args</c> is an optional dictionary
        /// of arguments for the type constructor.</para>
        /// <para>In addition to custom type names, <c>typeName</c> can be any of the
        /// predefined <c>Random</c>, <c>RoundRobin</c> or <c>Static</c> values.</para>
        /// <para>The default load balancer is the <see cref="RoundRobinLoadBalancer"/>.</para>
        /// </remarks>
        [BinderIgnore]
        public SingletonServiceFactory<ILoadBalancer> LoadBalancer { get; internal set; }
            = new SingletonServiceFactory<ILoadBalancer> { Creator = () => new RoundRobinLoadBalancer() };

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
                LoadBalancer.Creator = LoadBalancer.Service.CrateInstanceByName(value.TypeName, value.Args);
            }
        }
    }
}
