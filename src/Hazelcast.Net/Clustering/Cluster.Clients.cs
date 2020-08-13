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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;

namespace Hazelcast.Clustering
{
    internal partial class Cluster // Client Connections
    {
        /// <summary>
        /// Gets a random client connection.
        /// </summary>
        /// <param name="throwIfNoClient">Whether to throw if no client connection can be obtained immediately.</param>
        /// <returns>A random client connection.</returns>
        /// <remarks>
        /// <para>Throws if not client connection can be obtained immediately.</para>
        /// </remarks>
        internal ClientConnection GetRandomClientConnection(bool throwIfNoClient = true)
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
                    var memberId = _loadBalancer.GetMember();
                    if (_clientConnections.TryGetValue(memberId, out var lbclient))
                        return lbclient;
                }

                var clientConnection = _clientConnections.Values.FirstOrDefault();
                if (clientConnection == null && throwIfNoClient)
                    throw new HazelcastException("Could not get a client.");

                return clientConnection;
            }

            // there should be only one
            var singleClientConnection = _clientConnections.Values.FirstOrDefault();
            if (singleClientConnection == null && throwIfNoClient)
                throw new HazelcastException("Could not get a client.");
            return singleClientConnection;
        }

        /// <summary>
        /// Waits for a random client connection to be available and return it.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A random client connection.</returns>
        /// <remarks>
        /// <para>Tries to get a client connection for as long as <paramref name="cancellationToken"/> is not canceled.</para>
        /// </remarks>
        internal async ValueTask<ClientConnection> WaitRandomClientConnection(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // this is just basically retrieving a random client
                var clientConnection = GetRandomClientConnection(false);
                if (clientConnection != null) return clientConnection;

                // no need to try again if the client died
                if (_disposed == 1)
                    throw new ClientNotConnectedException();

                // no clients => wait for clients
                await Task.Delay(_options.Networking.WaitForClientMilliseconds, cancellationToken).CAF();
            }

            // this *will* throw
            cancellationToken.ThrowIfCancellationRequested();
            return default;
        }

        /// <summary>
        /// Gets a snapshot of the current client connections.
        /// </summary>
        /// <returns>A snapshot of the current client connections.</returns>
        internal IList<ClientConnection> SnapshotClientConnections()
            => _clientConnections.Values.ToList();
    }
}
