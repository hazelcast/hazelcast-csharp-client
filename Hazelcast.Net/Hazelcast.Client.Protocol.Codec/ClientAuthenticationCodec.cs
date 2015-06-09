using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ClientAuthenticationCodec
	{
		public static readonly ClientMessageType RequestType = ClientMessageType.ClientAuthentication;

		public const int ResponseType = 109;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly ClientMessageType Type = RequestType;

			public string username;

			public string password;

			public string uuid;

			public string ownerUuid;

			public bool isOwnerConnection;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string username, string password, string uuid, string ownerUuid, bool isOwnerConnection)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(username);
				dataSize += ParameterUtil.CalculateStringDataSize(password);
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

		public static ClientMessage EncodeRequest(string username, string password, string uuid, string ownerUuid, bool isOwnerConnection)
		{
			int requiredDataSize = ClientAuthenticationCodec.RequestParameters.CalculateDataSize(username, password, uuid, ownerUuid, isOwnerConnection);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(username);
			clientMessage.Set(password);
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

		public static ClientAuthenticationCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ClientAuthenticationCodec.RequestParameters parameters = new ClientAuthenticationCodec.RequestParameters();
			string username;
			username = null;
			username = clientMessage.GetStringUtf8();
			parameters.username = username;
			string password;
			password = null;
			password = clientMessage.GetStringUtf8();
			parameters.password = password;
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
			int requiredDataSize = ClientAuthenticationCodec.ResponseParameters.CalculateDataSize(address, uuid, ownerUuid);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			AddressCodec.Encode(address, clientMessage);
			clientMessage.Set(uuid);
			clientMessage.Set(ownerUuid);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientAuthenticationCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ClientAuthenticationCodec.ResponseParameters parameters = new ClientAuthenticationCodec.ResponseParameters();
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
