// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client.Network;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Spi
{
    internal partial class ClusterService : IConnectionListener
    {
        private readonly AtomicReference<Connection> _listenerAddedConnection = new AtomicReference<Connection>();

        public void ConnectionAdded(Connection connection)
        {
            TryRegister(connection);
        }

        public void ConnectionRemoved(Connection connection)
        {
            TryReRegisterToRandomConnection(connection);
        }

        private void TryReRegisterToRandomConnection(Connection oldConnection)
        {
            if (!_listenerAddedConnection.CompareAndSet(oldConnection, null))
            {
                //somebody else already trying to re-register
                return;
            }
            var newConnection = _connectionManager.GetRandomConnection();
            if (newConnection != null)
            {
                TryRegister(newConnection);
            }
        }

        private void TryRegister(Connection connection)
        {
            if (!_listenerAddedConnection.CompareAndSet(null, connection))
            {
                //already registering/registered to another connection
                return;
            }
            var clientMessage = ClientAddClusterViewListenerCodec.EncodeRequest();

            void HandlePartitionsViewEvent(int version, ICollection<KeyValuePair<Guid, IList<int>>> partitions) =>
                _partitionService.HandlePartitionsViewEvent(connection, partitions, version);

            void EventHandler(ClientMessage message) =>
                ClientAddClusterViewListenerCodec.EventHandler.HandleEvent(message, HandleMembersViewEvent,
                    HandlePartitionsViewEvent);

            IFuture<ClientMessage> future = _clientInvocationService.InvokeListenerOnConnection(clientMessage, EventHandler, connection);
            future.ToTask().ContinueWith(task =>
            {
                if (!task.IsFaulted)
                {
                    if (task.Result != null)
                    {
                        return;
                    }
                }
                //completes with exception, listener needs to be re-registered
                TryReRegisterToRandomConnection(connection);
            });
        }
    }
}