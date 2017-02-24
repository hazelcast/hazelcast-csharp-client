// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ClientAddMembershipListenerCodec
    {
        public const int ResponseType = 104;
        public const bool Retryable = false;

        public static readonly ClientMessageType RequestType = ClientMessageType.ClientAddMembershipListener;

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            string response = null;
            response = clientMessage.GetStringUtf8();
            parameters.response = response;
            return parameters;
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

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public string response;
        }


        //************************ EVENTS *************************//
        public abstract class AbstractEventHandler
        {
            public delegate void HandleMember(IMember member, int eventType);

            public delegate void HandleMemberAttributeChange(string uuid, string key, int operationType, string value);

            public delegate void HandleMemberSet(IList<IMember> members);

            public static void Handle(IClientMessage clientMessage, HandleMember handleMember,
                HandleMemberSet handleMemberSet, HandleMemberAttributeChange handleMemberAttributeChange)
            {
                var messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventMember)
                {
                    IMember member = null;
                    member = MemberCodec.Decode(clientMessage);
                    int eventType;
                    eventType = clientMessage.GetInt();
                    handleMember(member, eventType);
                    return;
                }
                if (messageType == EventMessageConst.EventMemberSet)
                {
                    IList<IMember> members = null;
                    var members_size = clientMessage.GetInt();
                    members = new List<IMember>();
                    for (var members_index = 0; members_index < members_size; members_index++)
                    {
                        IMember members_item;
                        members_item = MemberCodec.Decode(clientMessage);
                        members.Add(members_item);
                    }
                    handleMemberSet(members);
                    return;
                }
                if (messageType == EventMessageConst.EventMemberAttributeChange)
                {
                    string uuid = null;
                    uuid = clientMessage.GetStringUtf8();
                    string key = null;
                    key = clientMessage.GetStringUtf8();
                    int operationType;
                    operationType = clientMessage.GetInt();
                    string value = null;
                    var value_isNull = clientMessage.GetBoolean();
                    if (!value_isNull)
                    {
                        value = clientMessage.GetStringUtf8();
                    }
                    handleMemberAttributeChange(uuid, key, operationType, value);
                    return;
                }
                Logger.GetLogger(typeof (AbstractEventHandler))
                    .Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }
        }
    }
}