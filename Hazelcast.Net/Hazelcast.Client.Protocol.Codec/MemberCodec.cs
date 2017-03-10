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
using Hazelcast.Core;

namespace Hazelcast.Client.Protocol.Codec
{
    internal static class MemberCodec
    {
        public static Member Decode(IClientMessage clientMessage)
        {
            var address = AddressCodec.Decode(clientMessage);
            var uuid = clientMessage.GetStringUtf8();
            var liteMember = clientMessage.GetBoolean();
            var attributeSize = clientMessage.GetInt();
            IDictionary<string, string> attributes = new Dictionary<string, string>();
            for (var i = 0; i < attributeSize; i++)
            {
                var key = clientMessage.GetStringUtf8();
                var value = clientMessage.GetStringUtf8();
                attributes[key] = value;
            }
            return new Member(address, uuid, attributes, liteMember);
        }
    }
}