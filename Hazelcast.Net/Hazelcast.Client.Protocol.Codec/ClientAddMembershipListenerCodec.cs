// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Logging;

// Client Protocol version, Since:1.0 - Update:1.0
namespace Hazelcast.Client.Protocol.Codec
{
    internal static class ClientAddMembershipListenerCodec
    {
        private static int CalculateRequestDataSize(bool localOnly)
        {
            var dataSize = ClientMessage.HeaderSize;
            dataSize += Bits.BooleanSizeInBytes;
            return dataSize;
        }

        internal static ClientMessage EncodeRequest(bool localOnly)
        {
            var requiredDataSize = CalculateRequestDataSize(localOnly);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) ClientMessageType.ClientAddMembershipListener);
            clientMessage.SetRetryable(false);
            clientMessage.Set(localOnly);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        internal class ResponseParameters
        {
            public string response;
        }

        internal static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var response = clientMessage.GetStringUtf8();
            parameters.response = response;
            return parameters;
        }

        internal class EventHandler
        {
            internal static void HandleEvent(IClientMessage clientMessage, HandleMemberEventV10 handleMemberEventV10,
                HandleMemberListEventV10 handleMemberListEventV10,
                HandleMemberAttributeChangeEventV10 handleMemberAttributeChangeEventV10)
            {
                var messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventMember)
                {
                    var member = MemberCodec.Decode(clientMessage);
                    var eventType = clientMessage.GetInt();
                    handleMemberEventV10(member, eventType);
                    return;
                }
                if (messageType == EventMessageConst.EventMemberList)
                {
                    var membersSize = clientMessage.GetInt();
                    var members = new List<IMember>(membersSize);
                    for (var membersIndex = 0; membersIndex < membersSize; membersIndex++)
                    {
                        var membersItem = MemberCodec.Decode(clientMessage);
                        members.Add(membersItem);
                    }
                    handleMemberListEventV10(members);
                    return;
                }
                if (messageType == EventMessageConst.EventMemberAttributeChange)
                {
                    var uuid = clientMessage.GetStringUtf8();
                    var key = clientMessage.GetStringUtf8();
                    var operationType = clientMessage.GetInt();
                    string value = null;
                    var valueIsNull = clientMessage.GetBoolean();
                    if (!valueIsNull)
                    {
                        value = clientMessage.GetStringUtf8();
                    }
                    handleMemberAttributeChangeEventV10(uuid, key, operationType, value);
                    return;
                }
                Logger.GetLogger(typeof(EventHandler)).Warning("Unknown message type received on event handler :" + messageType);
            }

            internal delegate void HandleMemberEventV10(IMember member, int eventType);

            internal delegate void HandleMemberListEventV10(IList<IMember> members);

            internal delegate void HandleMemberAttributeChangeEventV10(string uuid, string key, int operationType, string value);
        }
    }
}