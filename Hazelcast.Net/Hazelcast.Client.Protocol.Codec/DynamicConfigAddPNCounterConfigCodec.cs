/*
 * Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol.Codec
{
    /*
    * This file is auto-generated by the Hazelcast Client Protocol Code Generator.
    * To change this file, edit the templates or the protocol
    * definitions on the https://github.com/hazelcast/hazelcast-client-protocol
    * and regenerate it.
    */

    /// <summary>
    /// Adds a new CRDT PN counter configuration to a running cluster.
    /// If a PN counter configuration with the given {@code name} already exists, then
    /// the new configuration is ignored and the existing one is preserved.
    ///</summary>
    internal static class DynamicConfigAddPNCounterConfigCodec 
    {
        public const int RequestMessageType = 0x1E1600;
        public const int ResponseMessageType = 0x1E1601;
        private const int RequestReplicaCountFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestStatisticsEnabledFieldOffset = RequestreplicaCountFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestStatisticsEnabledFieldOffset + BooleanSizeInBytes;
        private const int ResponseInitialFrameSize = CorrelationIdFieldOffset + LongSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// name of the CRDT PN counter configuration
            ///</summary>
            public string Name;

            /// <summary>
            /// number of replicas on which the CRDT state is kept
            ///</summary>
            public int ReplicaCount;

            /// <summary>
            /// set to {@code true} to enable statistics on this multimap configuration
            ///</summary>
            public bool StatisticsEnabled;

            /// <summary>
            /// name of an existing configured split brain protection to be used to determine the minimum number of members
            /// required in the cluster for the lock to remain functional. When {@code null}, split brain protection does not
            /// apply to this lock configuration's operations.
            ///</summary>
            public string SplitBrainProtectionName;
        }

        public static ClientMessage EncodeRequest(string name, int replicaCount, bool statisticsEnabled, string splitBrainProtectionName) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "DynamicConfig.AddPNCounterConfig";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame.Content, RequestReplicaCountFieldOffset, replicaCount);
            EncodeBool(initialFrame.Content, RequestStatisticsEnabledFieldOffset, statisticsEnabled);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            CodecUtil.EncodeNullable(clientMessage, splitBrainProtectionName, StringCodec.Encode);
            return clientMessage;
        }

        public static DynamicConfigAddPNCounterConfigCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.ReplicaCount =  DecodeInt(initialFrame.Content, RequestReplicaCountFieldOffset);
            request.StatisticsEnabled =  DecodeBool(initialFrame.Content, RequestStatisticsEnabledFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            request.SplitBrainProtectionName = CodecUtil.DecodeNullable(ref iterator, StringCodec.Decode);
            return request;
        }

        public class ResponseParameters 
        {
        }

        public static ClientMessage EncodeResponse() 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            return clientMessage;
        }

        public static DynamicConfigAddPNCounterConfigCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            return response;
        }
    }
}