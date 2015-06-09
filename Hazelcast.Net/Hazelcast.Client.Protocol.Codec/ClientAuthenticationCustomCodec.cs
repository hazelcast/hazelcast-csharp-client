using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ClientAuthenticationCustomCodec
	{
		public static readonly ClientMessageType RequestType = ClientMessageType.ClientAuthenticationcustom;

		public const int ResponseType = 109;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly ClientMessageType Type = RequestType;

			public IData credentials;

			public string uuid;

			public string ownerUuid;

			public bool isOwnerConnection;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(IData credentials, string uuid, string ownerUuid, bool isOwnerConnection)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateDataSize(credentials);
				dataSize += Bits.BooleanSizeInBytes;
				if (uuid != null)
				{
					dataSize += ParameterUtil.CalculateStringDataSize(uuid);
				}
				dataSize += Bits.BooleanSizeInBytes;
				if (ownerUuid != null)
				{
					dataSize += ParameterUtil.CalculateStringDataSize(ownerUuid);
				}
				dataSize += Bits.BooleanSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(IData credentials, string uuid, string ownerUuid, bool isOwnerConnection)
		{
			int requiredDataSize = ClientAuthenticationCustomCodec.RequestParameters.CalculateDataSize(credentials, uuid, ownerUuid, isOwnerConnection);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
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
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientAuthenticationCustomCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ClientAuthenticationCustomCodec.RequestParameters parameters = new ClientAuthenticationCustomCodec.RequestParameters();
			IData credentials;
			credentials = null;
			credentials = clientMessage.GetData();
			parameters.credentials = credentials;
			string uuid;
			uuid = null;
			bool uuid_isNull = clientMessage.GetBoolean();
			if (!uuid_isNull)
			{
				uuid = clientMessage.GetStringUtf8();
				parameters.uuid = uuid;
			}
			string ownerUuid;
			ownerUuid = null;
			bool ownerUuid_isNull = clientMessage.GetBoolean();
			if (!ownerUuid_isNull)
			{
				ownerUuid = clientMessage.GetStringUtf8();
				parameters.ownerUuid = ownerUuid;
			}
			bool isOwnerConnection;
			isOwnerConnection = clientMessage.GetBoolean();
			parameters.isOwnerConnection = isOwnerConnection;
			return parameters;
		}

		public class ResponseParameters
		{
			public Address address;

			public string uuid;

			public string ownerUuid;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(Address address, string uuid, string ownerUuid)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += AddressCodec.CalculateDataSize(address);
				dataSize += ParameterUtil.CalculateStringDataSize(uuid);
				dataSize += ParameterUtil.CalculateStringDataSize(ownerUuid);
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(Address address, string uuid, string ownerUuid)
		{
			int requiredDataSize = ClientAuthenticationCustomCodec.ResponseParameters.CalculateDataSize(address, uuid, ownerUuid);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			AddressCodec.Encode(address, clientMessage);
			clientMessage.Set(uuid);
			clientMessage.Set(ownerUuid);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientAuthenticationCustomCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ClientAuthenticationCustomCodec.ResponseParameters parameters = new ClientAuthenticationCustomCodec.ResponseParameters();
			Address address;
			address = null;
			address = AddressCodec.Decode(clientMessage);
			parameters.address = address;
			string uuid;
			uuid = null;
			uuid = clientMessage.GetStringUtf8();
			parameters.uuid = uuid;
			string ownerUuid;
			ownerUuid = null;
			ownerUuid = clientMessage.GetStringUtf8();
			parameters.ownerUuid = ownerUuid;
			return parameters;
		}
	}
}
