/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

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

		public static Address Decode(IClientMessage clientMessage)
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
			int dataSize = ParameterUtil.CalculateDataSize(address.GetHost());
			dataSize += Bits.IntSizeInBytes;
			return dataSize;
		}
	}
}
