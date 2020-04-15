// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Messaging;
using Hazelcast.Logging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using Hazelcast.Protocol.Portability;
using static Hazelcast.Protocol.Portability.Temp;

// <auto-generated>
//   This code was generated by a tool.
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Adds a delta to the PNCounter value. The delta may be negative for a
    /// subtraction.
    /// <p>
    /// The invocation will return the replica timestamps (vector clock) which
    /// can then be sent with the next invocation to keep session consistency
    /// guarantees.
    /// The target replica is determined by the {@code targetReplica} parameter.
    /// If smart routing is disabled, the actual member processing the client
    /// message may act as a proxy.
    ///</summary>
    internal static class PNCounterAddCodec
    {
        public const int RequestMessageType = 1901056; // 0x1D0200
        public const int ResponseMessageType = 1901057; // 0x1D0201
        private const int RequestDeltaFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestGetBeforeUpdateFieldOffset = RequestDeltaFieldOffset + LongSizeInBytes;
        private const int RequestTargetReplicaUUIDFieldOffset = RequestGetBeforeUpdateFieldOffset + BoolSizeInBytes;
        private const int RequestInitialFrameSize = RequestTargetReplicaUUIDFieldOffset + GuidSizeInBytes;
        private const int ResponseValueFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseReplicaCountFieldOffset = ResponseValueFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseReplicaCountFieldOffset + IntSizeInBytes;

        public static ClientMessage EncodeRequest(string name, long delta, bool getBeforeUpdate, ICollection<KeyValuePair<Guid, long>> replicaTimestamps, Guid targetReplicaUUID)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "PNCounter.Add";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeLong(initialFrame, RequestDeltaFieldOffset, delta);
            EncodeBool(initialFrame, RequestGetBeforeUpdateFieldOffset, getBeforeUpdate);
            EncodeGuid(initialFrame, RequestTargetReplicaUUIDFieldOffset, targetReplicaUUID);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            EntryListUUIDLongCodec.Encode(clientMessage, replicaTimestamps);
            return clientMessage;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// Value of the counter.
            ///</summary>
            public long Value;

            /// <summary>
            /// last observed replica timestamps (vector clock)
            ///</summary>
            public IList<KeyValuePair<Guid, long>> ReplicaTimestamps;

            /// <summary>
            /// Number of replicas that keep the state of this counter.
            ///</summary>
            public int ReplicaCount;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Value = DecodeLong(initialFrame, ResponseValueFieldOffset);
            response.ReplicaCount = DecodeInt(initialFrame, ResponseReplicaCountFieldOffset);
            response.ReplicaTimestamps = EntryListUUIDLongCodec.Decode(iterator);
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members