using System.Net.Sockets;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class AddressCodec
	{
		private AddressCodec()
		{
		}

		public static Address Decode(ClientMessage clientMessage)
		{
			string host = clientMessage.GetStringUtf8();
			int port = clientMessage.GetInt();
			try
			{
				return new Address(host, port);
			}
			catch (SocketException)
			{
				return null;
			}
		}

		public static void Encode(Address address, ClientMessage clientMessage)
		{
			clientMessage.Set(address.GetHost()).Set(address.GetPort());
		}

		public static int CalculateDataSize(Address address)
		{
			int dataSize = ParameterUtil.CalculateStringDataSize(address.GetHost());
			dataSize += Bits.IntSizeInBytes;
			return dataSize;
		}
	}
}
