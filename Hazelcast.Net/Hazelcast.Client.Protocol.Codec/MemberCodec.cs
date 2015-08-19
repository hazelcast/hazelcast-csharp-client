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

using System;
using System.Collections.Generic;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.Core;
using Hazelcast.IO;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MemberCodec
	{
		private MemberCodec()
		{
		}

		public static Member Decode(IClientMessage clientMessage)
		{
			Address address = AddressCodec.Decode(clientMessage);
			string uuid = clientMessage.GetStringUtf8();
			int attributeSize = clientMessage.GetInt();
            IDictionary<string, string> attributes = new Dictionary<string, string>();
			for (int i = 0; i < attributeSize; i++)
			{
				string key = clientMessage.GetStringUtf8();
				string value = clientMessage.GetStringUtf8();
				attributes[key] = value;
			}
			return new Member(address, uuid, attributes);
		}

		public static void Encode(IMember member, ClientMessage clientMessage)
		{
			//NOT REQUIRED ON CLIENT
		}

		public static int CalculateDataSize(IMember member)
		{
			throw new NotSupportedException("should not be called on client side");
		}
	}
}
