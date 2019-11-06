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
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Util
{
    internal class ClientMessageDecoder
    {
        // private readonly Connection _connection;
        // private ClientEndpointManager clientEndpointManager;

        private readonly Dictionary<long, ClientMessage> _builderBySessionIdMap = new Dictionary<long, ClientMessage>();
        private readonly Action<ClientMessage> _onMessage;
        private readonly int _maxMessageLength;
        private ClientMessageReader _activeReader;

        public ClientMessageDecoder(Action<ClientMessage> onMessage)
        {
            _onMessage = onMessage;
            //if (properties == null)
            //{
            //    properties = new HazelcastProperties((Properties)null);
            //}
            //clientEndpointManager = onMessage instanceof ClientEngine ? ((ClientEngine)onMessage).getEndpointManager() : null;
            //maxMessageLength = properties.getInteger(GroupProperty.CLIENT_PROTOCOL_UNVERIFIED_MESSAGE_BYTES);
            _maxMessageLength = int.MaxValue;
            _activeReader = new ClientMessageReader(_maxMessageLength);
            //this.connection = connection;
        }

        public void OnRead(ByteBuffer src)
        {
            while (src.HasRemaining())
            {
                var trusted = IsEndpointTrusted();
                var complete = _activeReader.ReadFrom(src, trusted);
                if (!complete)
                {
                    break;
                }

                var firstFrame = _activeReader.Message.Head;
                var flags = firstFrame.Flags;
                if (ClientMessage.IsFlagSet(flags, ClientMessage.UnfragmentedMessage))
                {
                    HandleMessage(_activeReader.Message);
                }
                else if (!trusted)
                {
                    throw new InvalidOperationException("Fragmented client messages are not allowed before the client is authenticated.");
                }
                else
                {
                    var frameIterator = _activeReader.Message.GetIterator();
                    //ignore the fragmentationFrame
                    frameIterator.Next();
                    var startFrame = frameIterator.Next();
                    var fragmentationId = Bits.ReadLongL(firstFrame.Content, ClientMessage.FragmentationIdOffset);
                    if (ClientMessage.IsFlagSet(flags, ClientMessage.BeginFragmentFlag))
                    {
                        _builderBySessionIdMap[fragmentationId] = ClientMessage.CreateForDecode(startFrame);
                    }
                    else if (ClientMessage.IsFlagSet(flags, ClientMessage.EndFragmentFlag))
                    {
                        var clientMessage = MergeIntoExistingClientMessage(fragmentationId);
                        HandleMessage(clientMessage);
                    }
                    else
                    {
                        MergeIntoExistingClientMessage(fragmentationId);
                    }
                }

                _activeReader = new ClientMessageReader(_maxMessageLength);
            }
        }

        private bool IsEndpointTrusted()
        {
            return false;
            //    if (clientEndpointManager == null || clientIsTrusted)
            //    {
            //        return true;
            //    }
            //    ClientEndpoint endpoint = clientEndpointManager.getEndpoint(connection);
            //    clientIsTrusted = endpoint != null && endpoint.isAuthenticated();
            //    return clientIsTrusted;
        }

        private ClientMessage MergeIntoExistingClientMessage(long fragmentationId)
        {
            var existingMessage = _builderBySessionIdMap[fragmentationId];
            existingMessage.Merge(_activeReader.Message);
            return existingMessage;
        }

        private void HandleMessage(ClientMessage clientMessage)
        {
            //clientMessage.setConnection(connection);
            //normalPacketsRead.inc();
            _onMessage(clientMessage);
        }
    }
}