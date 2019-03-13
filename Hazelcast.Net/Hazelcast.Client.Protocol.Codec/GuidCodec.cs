// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using System;

namespace Hazelcast.Client.Protocol.Codec
{
    internal static class GuidCodec
    {
        public static int CalculateDataSize(Guid address)
        {
            return 8;
        }

        public static Guid Decode(IClientMessage cm)
        {
            return new Guid(cm.GetInt(), cm.GetShort(), cm.GetShort(), cm.GetByte(), cm.GetByte(), cm.GetByte(), cm.GetByte(),
                cm.GetByte(), cm.GetByte(), cm.GetByte(), cm.GetByte());
        }

        public static void Encode(Guid uuid, ClientMessage clientMessage)
        {
            var bytes = uuid.ToByteArray();
            clientMessage.Set(bytes);
        }
    }
}