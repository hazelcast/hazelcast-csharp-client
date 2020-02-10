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
        private readonly Dictionary<long, ClientMessage> _builderBySessionIdMap = new Dictionary<long, ClientMessage>();
        private ClientMessageReader _activeReader = new ClientMessageReader();
        private readonly Action<ClientMessage> _onMessage;

        public ClientMessageDecoder(Action<ClientMessage> onMessage)
        {
            _onMessage = onMessage;
        }

        public void OnRead(ByteBuffer src)
        {
            while (src.HasRemaining())
            {
                var complete = _activeReader.ReadFrom(src);
                if (!complete)
                {
                    break;
                }

                var firstFrame = _activeReader.Message.FirstFrame;
                var flags = firstFrame.Flags;
                if (ClientMessage.IsFlagSet(flags, ClientMessage.UnfragmentedMessage))
                {
                    HandleMessage(_activeReader.Message);
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

                _activeReader = new ClientMessageReader();
            }
        }

        private ClientMessage MergeIntoExistingClientMessage(long fragmentationId)
        {
            var existingMessage = _builderBySessionIdMap[fragmentationId];
            existingMessage.Merge(_activeReader.Message);
            return existingMessage;
        }

        private void HandleMessage(ClientMessage clientMessage)
        {
            _onMessage(clientMessage);
        }
    }
}