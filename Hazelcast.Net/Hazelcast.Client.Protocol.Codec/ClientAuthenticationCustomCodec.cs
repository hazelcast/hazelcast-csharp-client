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
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

// Client Protocol version, Since:1.0 - Update:1.3

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ClientAuthenticationCustomCodec
    {
        public static readonly ClientMessageType RequestType = ClientMessageType.ClientAuthenticationCustom;
        public const int ResponseType = 107;
        public const bool Retryable = true;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ClientMessageType TYPE = RequestType;
            public IData credentials;
            public string uuid;
            public string ownerUuid;
            public bool isOwnerConnection;
            public string clientType;
            public byte serializationVersion;
            public string clientHazelcastVersion;

            public static int CalculateDataSize(IData credentials, string uuid, string ownerUuid, bool isOwnerConnection,
                string clientType, byte serializationVersion, string clientHazelcastVersion)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(credentials);
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
        }

        public static ClientMessage EncodeRequest(IData credentials, string uuid, string ownerUuid,
            bool isOwnerConnection, string clientType, byte serializationVersion, string clientHazelcastVersion)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(credentials, uuid, ownerUuid, isOwnerConnection,
                clientType, serializationVersion, clientHazelcastVersion);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(credentials);
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

        //************************ RESPONSE *************************//
        public class ResponseParameters
        {
            public byte status;
            public Address address;
            public string uuid;
            public string ownerUuid;
            public byte serializationVersion;
            public string serverHazelcastVersion;
            public bool serverHazelcastVersionExist;
            public IList<Core.IMember> clientUnregisteredMembers;
            public bool clientUnregisteredMembersExist;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var status = clientMessage.GetByte();
            parameters.status = status;
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
                var clientUnregisteredMembers = new List<Core.IMember>();
                var clientUnregisteredMembersSize = clientMessage.GetInt();
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