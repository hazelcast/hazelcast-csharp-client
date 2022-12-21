﻿// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

// <auto-generated>
//   This code was generated by a tool.
//   Hazelcast Client Protocol Code Generator @8aed6958e
//   https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantUsingDirective
// ReSharper disable CheckNamespace

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Query operation to retrieve the current value of the PNCounter.
    /// <p>
    /// The invocation will return the replica timestamps (vector clock) which
    /// can then be sent with the next invocation to keep session consistency
    /// guarantees.
    /// The target replica is determined by the {@code targetReplica} parameter.
    /// If smart routing is disabled, the actual member processing the client
    /// message may act as a proxy.
    ///</summary>
#if SERVER_CODEC
    internal static class PNCounterGetServerCodec
#else
    internal static class PNCounterGetCodec
#endif
    {
        public const int RequestMessageType = 1900800; // 0x1D0100
        public const int ResponseMessageType = 1900801; // 0x1D0101
        private const int RequestTargetReplicaUUIDFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestTargetReplicaUUIDFieldOffset + BytesExtensions.SizeOfCodecGuid;
        private const int ResponseValueFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseReplicaCountFieldOffset = ResponseValueFieldOffset + BytesExtensions.SizeOfLong;
        private const int ResponseInitialFrameSize = ResponseReplicaCountFieldOffset + BytesExtensions.SizeOfInt;

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// the name of the PNCounter
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// last observed replica timestamps (vector clock)
            ///</summary>
            public IList<KeyValuePair<Guid, long>> ReplicaTimestamps { get; set; }

            /// <summary>
            /// the target replica
            ///</summary>
            public Guid TargetReplicaUUID { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(string name, ICollection<KeyValuePair<Guid, long>> replicaTimestamps, Guid targetReplicaUUID)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = true,
                OperationName = "PNCounter.Get"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteGuidL(RequestTargetReplicaUUIDFieldOffset, targetReplicaUUID);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            EntryListUUIDLongCodec.Encode(clientMessage, replicaTimestamps);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.TargetReplicaUUID = initialFrame.Bytes.ReadGuidL(RequestTargetReplicaUUIDFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            request.ReplicaTimestamps = EntryListUUIDLongCodec.Decode(iterator);
            return request;
        }
#endif

        public sealed class ResponseParameters
        {

            /// <summary>
            /// Value of the counter.
            ///</summary>
            public long Value { get; set; }

            /// <summary>
            /// last observed replica timestamps (vector clock)
            ///</summary>
            public IList<KeyValuePair<Guid, long>> ReplicaTimestamps { get; set; }

            /// <summary>
            /// Number of replicas that keep the state of this counter.
            ///</summary>
            public int ReplicaCount { get; set; }
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse(long @value, ICollection<KeyValuePair<Guid, long>> replicaTimestamps, int replicaCount)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            initialFrame.Bytes.WriteLongL(ResponseValueFieldOffset, @value);
            initialFrame.Bytes.WriteIntL(ResponseReplicaCountFieldOffset, replicaCount);
            clientMessage.Append(initialFrame);
            EntryListUUIDLongCodec.Encode(clientMessage, replicaTimestamps);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Value = initialFrame.Bytes.ReadLongL(ResponseValueFieldOffset);
            response.ReplicaCount = initialFrame.Bytes.ReadIntL(ResponseReplicaCountFieldOffset);
            response.ReplicaTimestamps = EntryListUUIDLongCodec.Decode(iterator);
            return response;
        }

    }
}
