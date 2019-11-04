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
    /// Updates the config of a map.
    ///</summary>
    internal static class MCUpdateMapConfigCodec 
    {
        //hex: 0x200400
        public const int RequestMessageType = 2098176;
        //hex: 0x200401
        public const int ResponseMessageType = 2098177;
        private const int RequestTimeToLiveSecondsFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestMaxIdleSecondsFieldOffset = RequestTimeToLiveSecondsFieldOffset + IntSizeInBytes;
        private const int RequestEvictionPolicyFieldOffset = RequestMaxIdleSecondsFieldOffset + IntSizeInBytes;
        private const int RequestReadBackupDataFieldOffset = RequestEvictionPolicyFieldOffset + IntSizeInBytes;
        private const int RequestMaxSizeFieldOffset = RequestReadBackupDataFieldOffset + BoolSizeInBytes;
        private const int RequestMaxSizePolicyFieldOffset = RequestMaxSizeFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestMaxSizePolicyFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + IntSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the map.
            ///</summary>
            public string MapName;

            /// <summary>
            /// Time to live seconds for the map entries.
            ///</summary>
            public int TimeToLiveSeconds;

            /// <summary>
            /// Maximum idle seconds for the map entries.
            ///</summary>
            public int MaxIdleSeconds;

            /// <summary>
            /// The eviction policy of the map:
            /// 0 - LRU
            /// 1 - LFU
            /// 2 - NONE
            /// 3 - RANDOM
            ///</summary>
            public int EvictionPolicy;

            /// <summary>
            /// Whether reading from backup data is allowed.
            ///</summary>
            public bool ReadBackupData;

            /// <summary>
            /// Maximum size of the map.
            ///</summary>
            public int MaxSize;

            /// <summary>
            /// The maximum size policy of the map:
            /// 0 - PER_NODE
            /// 1 - PER_PARTITION
            /// 2 - USED_HEAP_PERCENTAGE
            /// 3 - USED_HEAP_SIZE
            /// 4 - FREE_HEAP_PERCENTAGE
            /// 5 - FREE_HEAP_SIZE
            /// 6 - USED_NATIVE_MEMORY_SIZE
            /// 7 - USED_NATIVE_MEMORY_PERCENTAGE
            /// 8 - FREE_NATIVE_MEMORY_SIZE
            /// 9 - FREE_NATIVE_MEMORY_PERCENTAGE
            ///</summary>
            public int MaxSizePolicy;
        }

        public static ClientMessage EncodeRequest(string mapName, int timeToLiveSeconds, int maxIdleSeconds, int evictionPolicy, bool readBackupData, int maxSize, int maxSizePolicy) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "MC.UpdateMapConfig";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame.Content, RequestTimeToLiveSecondsFieldOffset, timeToLiveSeconds);
            EncodeInt(initialFrame.Content, RequestMaxIdleSecondsFieldOffset, maxIdleSeconds);
            EncodeInt(initialFrame.Content, RequestEvictionPolicyFieldOffset, evictionPolicy);
            EncodeBool(initialFrame.Content, RequestReadBackupDataFieldOffset, readBackupData);
            EncodeInt(initialFrame.Content, RequestMaxSizeFieldOffset, maxSize);
            EncodeInt(initialFrame.Content, RequestMaxSizePolicyFieldOffset, maxSizePolicy);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, mapName);
            return clientMessage;
        }

        public static MCUpdateMapConfigCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.TimeToLiveSeconds =  DecodeInt(initialFrame.Content, RequestTimeToLiveSecondsFieldOffset);
            request.MaxIdleSeconds =  DecodeInt(initialFrame.Content, RequestMaxIdleSecondsFieldOffset);
            request.EvictionPolicy =  DecodeInt(initialFrame.Content, RequestEvictionPolicyFieldOffset);
            request.ReadBackupData =  DecodeBool(initialFrame.Content, RequestReadBackupDataFieldOffset);
            request.MaxSize =  DecodeInt(initialFrame.Content, RequestMaxSizeFieldOffset);
            request.MaxSizePolicy =  DecodeInt(initialFrame.Content, RequestMaxSizePolicyFieldOffset);
            request.MapName = StringCodec.Decode(ref iterator);
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

        public static MCUpdateMapConfigCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            return response;
        }
    }
}