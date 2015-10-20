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

using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapPutAllCodec
    {

        public static readonly MapMessageType RequestType = MapMessageType.MapPutAll;
        public const int ResponseType = 100;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public string name;
            public IDictionary<IData,IData> entries;

            public static int CalculateDataSize(string name, IDictionary<IData,IData> entries)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                foreach (var entry in entries) {
                    var key = entry.Key;
                    var val = entry.Value;
                dataSize += ParameterUtil.CalculateDataSize(key);
                dataSize += ParameterUtil.CalculateDataSize(val);
                }
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, IDictionary<IData,IData> entries)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, entries);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(entries.Count);
            foreach (var entry in entries) {
                var key = entry.Key;
                var val = entry.Value;
            clientMessage.Set(key);
            clientMessage.Set(val);
            }
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }
    }
}
