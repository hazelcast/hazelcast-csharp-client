// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;

// Client Protocol version, Since:1.0 - Update:1.0
namespace Hazelcast.Client.Protocol.Codec
{
    internal static class MapAddIndexCodec
    {
        private static int CalculateRequestDataSize(string name, string attribute, bool ordered)
        {
            var dataSize = ClientMessage.HeaderSize;
            dataSize += ParameterUtil.CalculateDataSize(name);
            dataSize += ParameterUtil.CalculateDataSize(attribute);
            dataSize += Bits.BooleanSizeInBytes;
            return dataSize;
        }

        internal static ClientMessage EncodeRequest(string name, string attribute, bool ordered)
        {
            var requiredDataSize = CalculateRequestDataSize(name, attribute, ordered);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) MapMessageType.MapAddIndex);
            clientMessage.SetRetryable(false);
            clientMessage.Set(name);
            clientMessage.Set(attribute);
            clientMessage.Set(ordered);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE IS EMPTY *****************//
    }
}