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
    /// Gets the config of a map.
    ///</summary>
    internal static class MCGetMapConfigCodec 
    {
        //hex: 0x200300
        public const int RequestMessageType = 2097920;
        //hex: 0x200301
        public const int ResponseMessageType = 2097921;
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseInMemoryFormatFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseBackupCountFieldOffset = ResponseInMemoryFormatFieldOffset + IntSizeInBytes;
        private const int ResponseAsyncBackupCountFieldOffset = ResponseBackupCountFieldOffset + IntSizeInBytes;
        private const int ResponseTimeToLiveSecondsFieldOffset = ResponseAsyncBackupCountFieldOffset + IntSizeInBytes;
        private const int ResponseMaxIdleSecondsFieldOffset = ResponseTimeToLiveSecondsFieldOffset + IntSizeInBytes;
        private const int ResponseMaxSizeFieldOffset = ResponseMaxIdleSecondsFieldOffset + IntSizeInBytes;
        private const int ResponseMaxSizePolicyFieldOffset = ResponseMaxSizeFieldOffset + IntSizeInBytes;
        private const int ResponseReadBackupDataFieldOffset = ResponseMaxSizePolicyFieldOffset + IntSizeInBytes;
        private const int ResponseEvictionPolicyFieldOffset = ResponseReadBackupDataFieldOffset + BoolSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseEvictionPolicyFieldOffset + IntSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the map.
            ///</summary>
            public string MapName;
        }

        public static ClientMessage EncodeRequest(string mapName) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "MC.GetMapConfig";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, mapName);
            return clientMessage;
        }

        public static MCGetMapConfigCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Next();
            request.MapName = StringCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// The in memory storage format of the map:
            /// 0 - Binary
            /// 1 - Object
            /// 2 - Native
            ///</summary>
            public int InMemoryFormat;

            /// <summary>
            /// Backup count of the map.
            ///</summary>
            public int BackupCount;

            /// <summary>
            /// Async backup count of the map.
            ///</summary>
            public int AsyncBackupCount;

            /// <summary>
            /// Time to live seconds for the map entries.
            ///</summary>
            public int TimeToLiveSeconds;

            /// <summary>
            /// Maximum idle seconds for the map entries.
            ///</summary>
            public int MaxIdleSeconds;

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

            /// <summary>
            /// Whether reading from backup data is allowed.
            ///</summary>
            public bool ReadBackupData;

            /// <summary>
            /// The eviction policy of the map:
            /// 0 - LRU
            /// 1 - LFU
            /// 2 - NONE
            /// 3 - RANDOM
            ///</summary>
            public int EvictionPolicy;

            /// <summary>
            /// Classname of the SplitBrainMergePolicy for the map.
            ///</summary>
            public string MergePolicy;
        }

        public static ClientMessage EncodeResponse(int inMemoryFormat, int backupCount, int asyncBackupCount, int timeToLiveSeconds, int maxIdleSeconds, int maxSize, int maxSizePolicy, bool readBackupData, int evictionPolicy, string mergePolicy) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeInt(initialFrame.Content, ResponseInMemoryFormatFieldOffset, inMemoryFormat);
            EncodeInt(initialFrame.Content, ResponseBackupCountFieldOffset, backupCount);
            EncodeInt(initialFrame.Content, ResponseAsyncBackupCountFieldOffset, asyncBackupCount);
            EncodeInt(initialFrame.Content, ResponseTimeToLiveSecondsFieldOffset, timeToLiveSeconds);
            EncodeInt(initialFrame.Content, ResponseMaxIdleSecondsFieldOffset, maxIdleSeconds);
            EncodeInt(initialFrame.Content, ResponseMaxSizeFieldOffset, maxSize);
            EncodeInt(initialFrame.Content, ResponseMaxSizePolicyFieldOffset, maxSizePolicy);
            EncodeBool(initialFrame.Content, ResponseReadBackupDataFieldOffset, readBackupData);
            EncodeInt(initialFrame.Content, ResponseEvictionPolicyFieldOffset, evictionPolicy);
            StringCodec.Encode(clientMessage, mergePolicy);
            return clientMessage;
        }

        public static MCGetMapConfigCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.InMemoryFormat = DecodeInt(initialFrame.Content, ResponseInMemoryFormatFieldOffset);
            response.BackupCount = DecodeInt(initialFrame.Content, ResponseBackupCountFieldOffset);
            response.AsyncBackupCount = DecodeInt(initialFrame.Content, ResponseAsyncBackupCountFieldOffset);
            response.TimeToLiveSeconds = DecodeInt(initialFrame.Content, ResponseTimeToLiveSecondsFieldOffset);
            response.MaxIdleSeconds = DecodeInt(initialFrame.Content, ResponseMaxIdleSecondsFieldOffset);
            response.MaxSize = DecodeInt(initialFrame.Content, ResponseMaxSizeFieldOffset);
            response.MaxSizePolicy = DecodeInt(initialFrame.Content, ResponseMaxSizePolicyFieldOffset);
            response.ReadBackupData = DecodeBool(initialFrame.Content, ResponseReadBackupDataFieldOffset);
            response.EvictionPolicy = DecodeInt(initialFrame.Content, ResponseEvictionPolicyFieldOffset);
            response.MergePolicy = StringCodec.Decode(ref iterator);
            return response;
        }
    }
}