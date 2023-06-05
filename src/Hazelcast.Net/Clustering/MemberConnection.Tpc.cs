// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Networking;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    internal partial class MemberConnection
    {
        private Task _connectingTpc;
        private CancellationTokenSource _connectingTpcCancellation;
        private IList<ClientMessageConnection> _tpcConnections;

        private void ConnectTpc(MemberConnection client, AuthenticationResult result)
        {
            // allow for a global timeout of 2s per port
            // TODO: should timeout be an option? what is a good value?
            var timeout = TimeSpan.FromSeconds(2 * result.TpcPorts.Count);

            _logger.IfDebug()?.LogDebug("TPC is enabled and ports were received, establish TPC connections.");
            HConsole.WriteLine(this, "WITH TIMEOUT " + timeout);
            _connectingTpcCancellation = new CancellationTokenSource(timeout);
            _connectingTpc = ConnectTpc(result.TpcPorts, result.TpcToken, _connectingTpcCancellation.Token);
        }

        private async Task ConnectTpc(IList<int> tpcPorts, byte[] tpcToken, CancellationToken cancellationToken)
        {
            var tpcConnections = new ClientMessageConnection[tpcPorts.Count];

            var connectionTasks = tpcPorts
                .TakeWhile(_ => !cancellationToken.IsCancellationRequested)
                .Select(async (port, index) =>
                {
                    var tpcConnection = await ConnectTpcPort(port, tpcToken, cancellationToken).CfAwait();
                    tpcConnections[index] = tpcConnection; // that is thread-safe, each task has a different index
                });

            try
            {
                // TODO: should the parallel count be an option? what is a good value?
                await ParallelRunner.Run(connectionTasks, new ParallelRunner.Options { Count = 4 }).CfAwait();
                cancellationToken.ThrowIfCancellationRequested();
                _logger.IfDebug()?.LogDebug("All TPC connections have been established.");
            }
            catch (Exception e)
            {
                if (_networkingOptions.Tpc.Required)
                {
                    _logger.IfWarning()?.LogWarning(e, "Exception while establishing TPC connections, terminating this member connection.");

                    // dispose this MemberConnection
                    await DisposeAsync().CfAwait();
                }
                else
                {
                    _logger.IfWarning()?.LogWarning(e, "Exception while establishing TPC connections, falling back to classic.");

                    // terminate TPC connections that may have been established - and only TPC
                    await DisposeTpcConnections(tpcConnections, true);
                    tpcConnections = null;
                }

                // do *not* throw, this is just a background task
                // and, TPC connections are going to be disposed below
            }

            var disposed = false;
            lock (_mutex)
            {
                if (_disposed) disposed = true;
                else _tpcConnections = tpcConnections;
            }

            // if we have been disposed before assigning _tpcConnections,
            // we have to dispose these connections ourselves.
            if (disposed) await DisposeTpcConnections(tpcConnections).CfAwait();
        }

        private static async Task DisposeTpcConnections(IEnumerable<ClientMessageConnection> connections, bool onlyTpc = false)
        {
            if (connections == null) return;

            foreach (var connection in connections)
            {
                if (connection != null) // tcpConnections is an array and some slots may still be null
                {
                    // if only TPC make sure that the socket connection going down does
                    // not propagate to the member connection by clearing the shutdown handler
                    if (onlyTpc) connection.SocketConnection.ClearOnShutdown();
                    await connection.DisposeAsync().CfAwait(); // does not throw
                }
            }
        }

        private async Task<ClientMessageConnection> ConnectTpcPort(int tpcPort, byte[] tpcToken, CancellationToken cancellationToken)
        {
            // note: there is no retry here, either it works, or we consider it will *not* work, ever

            // note: this.Address is the address we connected to, but it might be a public
            // address which we cannot feed into the AddressProvider map, so we *have* to
            // make sure we use the private address here.
            var tpcAddress = _addressProvider.Map(_privateAddress.WithPort(tpcPort));

            var endpoint = tpcAddress.IPEndPoint;
            _logger.IfDebug()?.LogDebug("Establish connection to TPC port {Port} at {EndPoint}.", tpcPort, endpoint);

            // TODO: remove this?
            // TPC currently does not support SSL
            var sslOptions = _sslOptions.Clone();
            sslOptions.Enabled = false;

            var id = Guid.NewGuid();
            var socketConnection = new ClientSocketConnection(id, endpoint, _networkingOptions, sslOptions, _loggerFactory);
            var messageConnection = new ClientMessageConnection(socketConnection, _loggerFactory) { OnReceiveMessage = ReceiveMessage };

            try
            {
                // connect
                await socketConnection.ConnectAsync(cancellationToken).CfAwait();

                // send protocol bytes
                var sent = await socketConnection.SendAsync(ClientProtocolInitBytes, ClientProtocolInitBytes.Length, cancellationToken).CfAwait();
                if (!sent) throw new ConnectionException("Failed to send TPC protocol bytes.");

                // authenticate
                var auth = await _authenticator.AuthenticateTpcAsync(this, messageConnection, _clientId, tpcToken, cancellationToken).CfAwait();
                if (!auth) throw new ConnectionException("Failed to authenticate TPC connection.");

                // from now on, if this connection goes down, it will take down the entire member connection
                // (if it just went down, OnSocketShutdown is invoked immediately, there's no race condition here)
                await socketConnection.AddOnShutdown(OnSocketShutdown).CfAwait();

                _logger.IfDebug()?.LogDebug("Established client TPC connection {Id}/{TpcId} to {Address}:{Port}.", Id.ToShortString(), id.ToShortString(), Address.IPAddress, tpcPort);
            }
            catch
            {
                await messageConnection.DisposeAsync().CfAwait();
                throw;
            }

            return messageConnection;
        }
    }
}
