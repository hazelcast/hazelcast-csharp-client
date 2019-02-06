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

using System;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.IO.Serialization;

// Client Protocol version, Since:1.6 - Update:1.6
namespace Hazelcast.Client.Protocol.Codec
{
    internal static class PNCounterAddCodec
    {
        private static int CalculateRequestDataSize(string name, long delta, bool getBeforeUpdate, IList<KeyValuePair<string, long>> replicaTimestamps, Address targetReplica)
        {
            var dataSize = ClientMessage.HeaderSize;

            dataSize += ParameterUtil.CalculateDataSize(name);
            dataSize += Bits.LongSizeInBytes;
            dataSize += Bits.BooleanSizeInBytes;
            dataSize += Bits.IntSizeInBytes;

            foreach (var replicaTimestampsItem in replicaTimestamps )
            {
                var replicaTimestampsItemKey = replicaTimestampsItem.Key;
                var replicaTimestampsItemVal = replicaTimestampsItem.Value;

                dataSize += ParameterUtil.CalculateDataSize(replicaTimestampsItemKey);
                dataSize += ParameterUtil.CalculateDataSize(replicaTimestampsItemVal);
            }

            dataSize += AddressCodec.CalculateDataSize(targetReplica);
            return dataSize;
        }

        internal static ClientMessage EncodeRequest(string name, long delta, bool getBeforeUpdate, IList<KeyValuePair<string, long>> replicaTimestamps, Address targetReplica)
        {
            var requiredDataSize = CalculateRequestDataSize(name, delta, getBeforeUpdate, replicaTimestamps, targetReplica);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);

            clientMessage.SetMessageType((int)PNCounterMessageType.PNCounterAdd);
            clientMessage.SetRetryable(false);
            clientMessage.Set(name);
            clientMessage.Set(delta);
            clientMessage.Set(getBeforeUpdate);
            clientMessage.Set(replicaTimestamps.Count);

            foreach (var replicaTimestampsItem in replicaTimestamps)
            {
                var replicaTimestampsItemKey = replicaTimestampsItem.Key;
                var replicaTimestampsItemVal = replicaTimestampsItem.Value;

                clientMessage.Set(replicaTimestampsItemKey);
                clientMessage.Set(replicaTimestampsItemVal);
            }

            AddressCodec.Encode(targetReplica, clientMessage);
            clientMessage.UpdateFrameLength();

            return clientMessage;
        }

        internal class ResponseParameters
        {
            public long value;
            public IList<KeyValuePair<string, long>> replicaTimestamps;
            public int replicaCount;
        }

        internal static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var value = clientMessage.GetLong();
            parameters.value = value;

            var replicaTimestampsSize = clientMessage.GetInt();
            var replicaTimestamps = new List<KeyValuePair<string, long>>(replicaTimestampsSize);

            for (var replicaTimestampsIndex = 0; replicaTimestampsIndex<replicaTimestampsSize; replicaTimestampsIndex++)
            {
                var replicaTimestampsItemKey = clientMessage.GetStringUtf8();
                var replicaTimestampsItemVal = clientMessage.GetLong();
                var replicaTimestampsItem = new KeyValuePair<string,long>(replicaTimestampsItemKey, replicaTimestampsItemVal);

                replicaTimestamps.Add(replicaTimestampsItem);
            }

            parameters.replicaTimestamps = replicaTimestamps;
            var replicaCount = clientMessage.GetInt();
            parameters.replicaCount = replicaCount;
            return parameters;
        }
    }
}
