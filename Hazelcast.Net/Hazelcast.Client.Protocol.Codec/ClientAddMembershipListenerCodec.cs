// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.IO;

// Client Protocol version, Since:1.0 - Update:1.0

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ClientAddMembershipListenerCodec
    {
        public static readonly ClientMessageType RequestType = ClientMessageType.ClientAddMembershipListener;
        public const int ResponseType = 104;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ClientMessageType TYPE = RequestType;
            public bool localOnly;

            public static int CalculateDataSize(bool localOnly)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += Bits.BooleanSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(bool localOnly)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(localOnly);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(localOnly);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//
        public class ResponseParameters
        {
            public string response;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var response = clientMessage.GetStringUtf8();
            parameters.response = response;
            return parameters;
        }

//************************ EVENTS *************************//
        public abstract class AbstractEventHandler
        {
            public static void Handle(IClientMessage clientMessage, HandleMember handleMember,
                HandleMemberList handleMemberList, HandleMemberAttributeChange handleMemberAttributeChange)
            {
                var messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventMember)
                {
                    var member = MemberCodec.Decode(clientMessage);
                    var eventType = clientMessage.GetInt();
                    handleMember(member, eventType);
                    return;
                }
                if (messageType == EventMessageConst.EventMemberList)
                {
                    var members = new List<Core.IMember>();
                    var membersSize = clientMessage.GetInt();
                    for (var membersIndex = 0; membersIndex < membersSize; membersIndex++)
                    {
                        var membersItem = MemberCodec.Decode(clientMessage);
                        members.Add(membersItem);
                    }
                    handleMemberList(members);
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
                    handleMemberAttributeChange(uuid, key, operationType, value);
                    return;
                }
                Hazelcast.Logging.Logger.GetLogger(typeof(AbstractEventHandler))
                    .Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }

            public delegate void HandleMember(Core.IMember member, int eventType);

            public delegate void HandleMemberList(IList<Core.IMember> members);

            public delegate void HandleMemberAttributeChange(string uuid, string key, int operationType, string value);
        }
    }
}