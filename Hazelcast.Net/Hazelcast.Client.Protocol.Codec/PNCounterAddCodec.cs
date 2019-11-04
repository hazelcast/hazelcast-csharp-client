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
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Codec.Custom;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol.Codec
{
    // This file is auto-generated by the Hazelcast Client Protocol Code Generator.
    // To change this file, edit the templates or the protocol
    // definitions on the https://github.com/hazelcast/hazelcast-client-protocol
    // and regenerate it.

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
        //hex: 0x1D0200
        public const int RequestMessageType = 1901056;
        //hex: 0x1D0201
        public const int ResponseMessageType = 1901057;
        private const int RequestDeltaFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestGetBeforeUpdateFieldOffset = RequestDeltaFieldOffset + LongSizeInBytes;
        private const int RequestInitialFrameSize = RequestGetBeforeUpdateFieldOffset + BoolSizeInBytes;
        private const int ResponseValueFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseReplicaCountFieldOffset = ResponseValueFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseReplicaCountFieldOffset + IntSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// the name of the PNCounter
            ///</summary>
            public string Name;

            /// <summary>
            /// the delta to add to the counter value, can be negative
            ///</summary>
            public long Delta;

            /// <summary>
            /// {@code true} if the operation should return the
            /// counter value before the addition, {@code false}
            /// if it should return the value after the addition
            ///</summary>
            public bool GetBeforeUpdate;

            /// <summary>
            /// last observed replica timestamps (vector clock)
            ///</summary>
            public IList<KeyValuePair<Guid, long>> ReplicaTimestamps;

            /// <summary>
            /// the target replica
            ///</summary>
            public IO.Address TargetReplica;
        }

        public static ClientMessage EncodeRequest(string name, long delta, bool getBeforeUpdate, IEnumerable<KeyValuePair<Guid, long>> replicaTimestamps, IO.Address targetReplica) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "PNCounter.Add";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeLong(initialFrame.Content, RequestDeltaFieldOffset, delta);
            EncodeBool(initialFrame.Content, RequestGetBeforeUpdateFieldOffset, getBeforeUpdate);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            EntryListUUIDLongCodec.Encode(clientMessage, replicaTimestamps);
            AddressCodec.Encode(clientMessage, targetReplica);
            return clientMessage;
        }

        public static PNCounterAddCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.Delta =  DecodeLong(initialFrame.Content, RequestDeltaFieldOffset);
            request.GetBeforeUpdate =  DecodeBool(initialFrame.Content, RequestGetBeforeUpdateFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            request.ReplicaTimestamps = EntryListUUIDLongCodec.Decode(ref iterator);
            request.TargetReplica = AddressCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long Value;

            /// <summary>
            /// last observed replica timestamps (vector clock)
            ///</summary>
            public IList<KeyValuePair<Guid, long>> ReplicaTimestamps;

            /// <summary>
            /// TODO DOC
            ///</summary>
            public int ReplicaCount;
        }

        public static ClientMessage EncodeResponse(long value, IEnumerable<KeyValuePair<Guid, long>> replicaTimestamps, int replicaCount) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeLong(initialFrame.Content, ResponseValueFieldOffset, value);
            EncodeInt(initialFrame.Content, ResponseReplicaCountFieldOffset, replicaCount);
            EntryListUUIDLongCodec.Encode(clientMessage, replicaTimestamps);
            return clientMessage;
        }

        public static PNCounterAddCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Value = DecodeLong(initialFrame.Content, ResponseValueFieldOffset);
            response.ReplicaCount = DecodeInt(initialFrame.Content, ResponseReplicaCountFieldOffset);
            response.ReplicaTimestamps = EntryListUUIDLongCodec.Decode(ref iterator);
            return response;
        }
    }
}