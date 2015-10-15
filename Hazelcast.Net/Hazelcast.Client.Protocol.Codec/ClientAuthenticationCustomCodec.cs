using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

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

            public static int CalculateDataSize(IData credentials, string uuid, string ownerUuid, bool isOwnerConnection, string clientType, byte serializationVersion)
            {
                int dataSize = ClientMessage.HeaderSize;
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
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(IData credentials, string uuid, string ownerUuid, bool isOwnerConnection, string clientType, byte serializationVersion)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(credentials, uuid, ownerUuid, isOwnerConnection, clientType, serializationVersion);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(credentials);
            bool uuid_isNull;
            if (uuid == null)
            {
                uuid_isNull = true;
                clientMessage.Set(uuid_isNull);
            }
            else
            {
                uuid_isNull = false;
                clientMessage.Set(uuid_isNull);
                clientMessage.Set(uuid);
            }
            bool ownerUuid_isNull;
            if (ownerUuid == null)
            {
                ownerUuid_isNull = true;
                clientMessage.Set(ownerUuid_isNull);
            }
            else
            {
                ownerUuid_isNull = false;
                clientMessage.Set(ownerUuid_isNull);
                clientMessage.Set(ownerUuid);
            }
            clientMessage.Set(isOwnerConnection);
            clientMessage.Set(clientType);
            clientMessage.Set(serializationVersion);
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
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            byte status;
            status = clientMessage.GetByte();
            parameters.status = status;
            Address address = null;
            bool address_isNull = clientMessage.GetBoolean();
            if (!address_isNull)
            {
                address = AddressCodec.Decode(clientMessage);
                parameters.address = address;
            }
            string uuid = null;
            bool uuid_isNull = clientMessage.GetBoolean();
            if (!uuid_isNull)
            {
                uuid = clientMessage.GetStringUtf8();
                parameters.uuid = uuid;
            }
            string ownerUuid = null;
            bool ownerUuid_isNull = clientMessage.GetBoolean();
            if (!ownerUuid_isNull)
            {
                ownerUuid = clientMessage.GetStringUtf8();
                parameters.ownerUuid = ownerUuid;
            }
            byte serializationVersion;
            serializationVersion = clientMessage.GetByte();
            parameters.serializationVersion = serializationVersion;
            return parameters;
        }

    }
}
