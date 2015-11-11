// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Util
{
    /// <summary>
    ///     Builds
    ///     <see cref="Hazelcast.Client.Protocol.ClientMessage" />
    ///     s from byte chunks. Fragmented messages are merged into single messages before processed.
    /// </summary>
    internal class ClientMessageBuilder
    {
        /// <summary>Implementers will be responsible to delegate the constructed message</summary>
        public delegate void HandleMessageDelegate(ClientMessage message);

        private readonly Dictionary<int, BufferBuilder> _builderBySessionIdMap = new Dictionary<int, BufferBuilder>();

        private readonly HandleMessageDelegate _delegate;
        private ClientMessage _message = ClientMessage.Create();

        public ClientMessageBuilder(HandleMessageDelegate _delegate)
        {
            this._delegate = _delegate;
        }

        public virtual void OnData(ByteBuffer buffer)
        {
            while (buffer.HasRemaining())
            {
                var complete = _message.ReadFrom(buffer);
                if (!complete)
                {
                    return;
                }
                //MESSAGE IS COMPLETE HERE
                if (_message.IsFlagSet(ClientMessage.BeginAndEndFlags))
                {
                    //HANDLE-MESSAGE
                    HandleMessage(_message);
                    _message = ClientMessage.Create();
                    continue;
                }
                if (_message.IsFlagSet(ClientMessage.BeginFlag))
                {
                    // first fragment
                    var builder = new BufferBuilder();
                    _builderBySessionIdMap.Add(_message.GetCorrelationId(), builder);
                    builder.Append(_message.GetBuffer(), 0, _message.GetFrameLength());
                }
                else
                {
                    var builder = _builderBySessionIdMap[_message.GetCorrelationId()];
                    if (builder.Position() == 0)
                    {
                        throw new InvalidOperationException();
                    }
                    builder.Append(_message.GetBuffer(), _message.GetDataOffset(),
                        _message.GetFrameLength() - _message.GetDataOffset());
                    if (_message.IsFlagSet(ClientMessage.EndFlag))
                    {
                        var msgLength = builder.Position();
                        var cm = ClientMessage.CreateForDecode(builder.Buffer(), 0);
                        cm.SetFrameLength(msgLength);
                        //HANDLE-MESSAGE
                        HandleMessage(cm);
                        _builderBySessionIdMap.Remove(_message.GetCorrelationId());
                    }
                }
                _message = ClientMessage.Create();
            }
        }

        private void HandleMessage(ClientMessage message)
        {
            message.Index(message.GetDataOffset());
            _delegate(message);
        }
    }
}