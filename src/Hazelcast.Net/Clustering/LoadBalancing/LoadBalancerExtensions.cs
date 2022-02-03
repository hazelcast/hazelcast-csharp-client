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
using System.Collections.Generic;
using System.Text;
using Hazelcast.Core;
using Hazelcast.Exceptions;

namespace Hazelcast.Clustering.LoadBalancing
{
    internal static class LoadBalancerExtensions
    {
        /// <summary>
        /// Extension method for default load balancer creators
        /// </summary>
        /// <param name="typeName">Predefined load balancer. RANDOM,ROUNDROBIN,STATIC or null for external</param>
        /// <param name="args">to pass <see cref="ILoadBalancer"/></param>
        /// <returns>Creator for <see cref="ILoadBalancer"/></returns>
        /// <exception cref="ArgumentException"></exception>
        internal static Func<ILoadBalancer> CrateInstanceByName(this ILoadBalancer loadBalancer, string typeName, Dictionary<string, string> args)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(typeName));
            
            return typeName.ToUpperInvariant() switch
            {
                "RANDOM" => () => new RandomLoadBalancer(),
                "ROUNDROBIN" => () => new RoundRobinLoadBalancer(),
                "STATIC" => () => new StaticLoadBalancer(args),
                _ => () => ServiceFactory.CreateInstance<ILoadBalancer>(typeName, args)
            };
        }
    }
}
