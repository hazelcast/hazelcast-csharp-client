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
using Hazelcast.Client.Protocol.Util;
using Hazelcast.Core;
using Hazelcast.IO;

// Client Protocol version, Since:1.0 - Update:1.3
namespace Hazelcast.Client.Protocol.Codec
{
    internal static class ClientAuthenticationCodec
    {
        private static int CalculateRequestDataSize(string username, string password, string uuid, string ownerUuid,
            bool isOwnerConnection, string clientType, byte serializationVersion, string clientHazelcastVersion)
        {
            var dataSize = ClientMessage.HeaderSize;
            dataSize += ParameterUtil.CalculateDataSize(username);
            dataSize += ParameterUtil.CalculateDataSize(password);
            dataSize += Bits.BooleanSizeInBytes;
            if (uuid != null)
            {
                dataSize += ParameterUtil.CalculateDataSize(uuid);
            }
            dataSize += Bits.BooleanSizeInBytes;
            if (ownerUuid != null)
            {
                dataSize += ParameterUtil.CalculateDataSize(ownerUuid);
            }
            dataSize += Bits.BooleanSizeInBytes;
            dataSize += ParameterUtil.CalculateDataSize(clientType);
            dataSize += Bits.ByteSizeInBytes;
            dataSize += ParameterUtil.CalculateDataSize(clientHazelcastVersion);
            return dataSize;
        }

        internal static ClientMessage EncodeRequest(string username, string password, string uuid, string ownerUuid,
            bool isOwnerConnection, string clientType, byte serializationVersion, string clientHazelcastVersion)
        {
            var requiredDataSize = CalculateRequestDataSize(username, password, uuid, ownerUuid, isOwnerConnection, clientType,
                serializationVersion, clientHazelcastVersion);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) ClientMessageType.ClientAuthentication);
            clientMessage.SetRetryable(true);
            clientMessage.Set(username);
            clientMessage.Set(password);
            clientMessage.Set(uuid == null);
            if (uuid != null)
            {
                clientMessage.Set(uuid);
            }
            clientMessage.Set(ownerUuid == null);
            if (ownerUuid != null)
            {
                clientMessage.Set(ownerUuid);
            }
            clientMessage.Set(isOwnerConnection);
            clientMessage.Set(clientType);
            clientMessage.Set(serializationVersion);
            clientMessage.Set(clientHazelcastVersion);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        internal class ResponseParameters
        {
            public AuthenticationStatus status;
            public Address address;
            public string uuid;
            public string ownerUuid;
            public byte serializationVersion;
            public string serverHazelcastVersion;
            public bool serverHazelcastVersionExist;
            public IList<IMember> clientUnregisteredMembers;
            public bool clientUnregisteredMembersExist;
        }

        internal static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var status = clientMessage.GetByte();
            parameters.status = (AuthenticationStatus)status;
            var addressIsNull = clientMessage.GetBoolean();
            if (!addressIsNull)
            {
                var address = AddressCodec.Decode(clientMessage);
                parameters.address = address;
            }
            var uuidIsNull = clientMessage.GetBoolean();
            if (!uuidIsNull)
            {
                var uuid = clientMessage.GetStringUtf8();
                parameters.uuid = uuid;
            }
            var ownerUuidIsNull = clientMessage.GetBoolean();
            if (!ownerUuidIsNull)
            {
                var ownerUuid = clientMessage.GetStringUtf8();
                parameters.ownerUuid = ownerUuid;
            }
            var serializationVersion = clientMessage.GetByte();
            parameters.serializationVersion = serializationVersion;
            if (clientMessage.IsComplete())
            {
                return parameters;
            }
            var serverHazelcastVersion = clientMessage.GetStringUtf8();
            parameters.serverHazelcastVersion = serverHazelcastVersion;
            parameters.serverHazelcastVersionExist = true;
            var clientUnregisteredMembersIsNull = clientMessage.GetBoolean();
            if (!clientUnregisteredMembersIsNull)
            {
                var clientUnregisteredMembersSize = clientMessage.GetInt();
                var clientUnregisteredMembers = new List<IMember>(clientUnregisteredMembersSize);
                for (var clientUnregisteredMembersIndex = 0;
                    clientUnregisteredMembersIndex < clientUnregisteredMembersSize;
                    clientUnregisteredMembersIndex++)
                {
                    var clientUnregisteredMembersItem = MemberCodec.Decode(clientMessage);
                    clientUnregisteredMembers.Add(clientUnregisteredMembersItem);
                }
                parameters.clientUnregisteredMembers = clientUnregisteredMembers;
            }
            parameters.clientUnregisteredMembersExist = true;
            return parameters;
        }
    }
}