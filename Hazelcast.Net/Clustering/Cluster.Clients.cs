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

using System.Linq;
using Hazelcast.Exceptions;

namespace Hazelcast.Clustering
{
    public partial class Cluster // Clients
    {
        /// <summary>
        /// Gets a random client.
        /// </summary>
        /// <returns>A random client.</returns>
        private Client GetRandomClient()
        {
            // In "smart mode" the clients connect to each member of the cluster. Since each
            // data partition uses the well known and consistent hashing algorithm, each client
            // can send an operation to the relevant cluster member, which increases the
            // overall throughput and efficiency. Smart mode is the default mode.
            //
            // In "uni-socket mode" the clients is required to connect to a single member, which
            // then behaves as a gateway for the other members. Firewalls, security, or some
            // custom networking issues can be the reason for these cases.

            var maxTries = _loadBalancer.Count;

            if (IsSmartRouting)
            {
                for (var i = 0; i < maxTries; i++)
                {
                    var memberId = _loadBalancer.Select();
                    if (_clients.TryGetValue(memberId, out var lbclient))
                        return lbclient;
                }

                var client = _clients.Values.FirstOrDefault();
                if (client == null)
                    throw new HazelcastException("Could not get a client.");

                return client;
            }

            // there should be only one
            var singleClient = _clients.Values.FirstOrDefault();
            if (singleClient == null)
                throw new HazelcastException("Could not get a client.");
            return singleClient;
        }
    }
}
