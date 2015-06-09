using Hazelcast.Client.Protocol;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ClientGetPartitionsCodec
	{
		public static readonly ClientMessageType RequestType = ClientMessageType.ClientGetpartitions;

		public const int ResponseType = 110;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ClientMessageType Type = RequestType;

			//************************ REQUEST *************************//
			public static int CalculateDataSize()
			{
				int dataSize = ClientMessage.HeaderSize;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest()
		{
			int requiredDataSize = ClientGetPartitionsCodec.RequestParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientGetPartitionsCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ClientGetPartitionsCodec.RequestParameters parameters = new ClientGetPartitionsCodec.RequestParameters();
			return parameters;
		}

		public class ResponseParameters
		{
			public Address[] members;

			public int[] ownerIndexes;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(Address[] members, int[] ownerIndexes)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += Bits.IntSizeInBytes;
				foreach (Address members_item in members)
				{
					dataSize += AddressCodec.CalculateDataSize(members_item);
				}
				dataSize += Bits.IntSizeInBytes;
				foreach (int ownerIndexes_item in ownerIndexes)
				{
					dataSize += Bits.IntSizeInBytes;
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(Address[] members, int[] ownerIndexes)
		{
			int requiredDataSize = ClientGetPartitionsCodec.ResponseParameters.CalculateDataSize(members, ownerIndexes);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(members.Length);
			foreach (Address members_item in members)
			{
				AddressCodec.Encode(members_item, clientMessage);
			}
			clientMessage.Set(ownerIndexes.Length);
			foreach (int ownerIndexes_item in ownerIndexes)
			{
				clientMessage.Set(ownerIndexes_item);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientGetPartitionsCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ClientGetPartitionsCodec.ResponseParameters parameters = new ClientGetPartitionsCodec.ResponseParameters();
			Address[] members;
			members = null;
			int members_size = clientMessage.GetInt();
			members = new Address[members_size];
			for (int members_index = 0; members_index < members_size; members_index++)
			{
				Address members_item = AddressCodec.Decode(clientMessage);
				members[members_index] = members_item;
			}
			parameters.members = members;
			int[] ownerIndexes;
			ownerIndexes = null;
			int ownerIndexes_size = clientMessage.GetInt();
			ownerIndexes = new int[ownerIndexes_size];
			for (int ownerIndexes_index = 0; ownerIndexes_index < ownerIndexes_size; ownerIndexes_index++)
			{
				int ownerIndexes_item = clientMessage.GetInt();
				ownerIndexes[ownerIndexes_index] = ownerIndexes_item;
			}
			parameters.ownerIndexes = ownerIndexes;
			return parameters;
		}
	}
}
