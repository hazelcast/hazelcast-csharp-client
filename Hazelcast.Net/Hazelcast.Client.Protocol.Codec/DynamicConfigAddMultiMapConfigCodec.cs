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
    /// Adds a new multimap config to a running cluster.
    /// If a multimap configuration with the given {@code name} already exists, then
    /// the new multimap config is ignored and the existing one is preserved.
    ///</summary>
    internal static class DynamicConfigAddMultiMapConfigCodec 
    {
        //hex: 0x1E0100
        public const int RequestMessageType = 1966336;
        //hex: 0x1E0101
        public const int ResponseMessageType = 1966337;
        private const int RequestBinaryFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestBackupCountFieldOffset = RequestBinaryFieldOffset + BoolSizeInBytes;
        private const int RequestAsyncBackupCountFieldOffset = RequestBackupCountFieldOffset + IntSizeInBytes;
        private const int RequestStatisticsEnabledFieldOffset = RequestAsyncBackupCountFieldOffset + IntSizeInBytes;
        private const int RequestMergeBatchSizeFieldOffset = RequestStatisticsEnabledFieldOffset + BoolSizeInBytes;
        private const int RequestInitialFrameSize = RequestMergeBatchSizeFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + IntSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// multimap configuration name
            ///</summary>
            public string Name;

            /// <summary>
            /// value collection type. Valid values are SET and LIST.
            ///</summary>
            public string CollectionType;

            /// <summary>
            /// entry listener configurations
            ///</summary>
            public IEnumerable<com.hazelcast.client.impl.protocol.task.dynamicconfig.ListenerConfigHolder> ListenerConfigs;

            /// <summary>
            /// {@code true} to store values in {@code BINARY} format or {@code false} to store
            /// values in {@code OBJECT} format.
            ///</summary>
            public bool Binary;

            /// <summary>
            /// number of synchronous backups
            ///</summary>
            public int BackupCount;

            /// <summary>
            /// number of asynchronous backups
            ///</summary>
            public int AsyncBackupCount;

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

            /// <summary>
            /// TODO DOC
            ///</summary>
            public string MergePolicy;

            /// <summary>
            /// TODO DOC
            ///</summary>
            public int MergeBatchSize;
        }

        public static ClientMessage EncodeRequest(string name, string collectionType, IEnumerable<com.hazelcast.client.impl.protocol.task.dynamicconfig.ListenerConfigHolder> listenerConfigs, bool binary, int backupCount, int asyncBackupCount, bool statisticsEnabled, string splitBrainProtectionName, string mergePolicy, int mergeBatchSize) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "DynamicConfig.AddMultiMapConfig";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeBool(initialFrame.Content, RequestBinaryFieldOffset, binary);
            EncodeInt(initialFrame.Content, RequestBackupCountFieldOffset, backupCount);
            EncodeInt(initialFrame.Content, RequestAsyncBackupCountFieldOffset, asyncBackupCount);
            EncodeBool(initialFrame.Content, RequestStatisticsEnabledFieldOffset, statisticsEnabled);
            EncodeInt(initialFrame.Content, RequestMergeBatchSizeFieldOffset, mergeBatchSize);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            StringCodec.Encode(clientMessage, collectionType);
            ListMultiFrameCodec.EncodeNullable(clientMessage, listenerConfigs, ListenerConfigHolderCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, splitBrainProtectionName, StringCodec.Encode);
            StringCodec.Encode(clientMessage, mergePolicy);
            return clientMessage;
        }

        public static DynamicConfigAddMultiMapConfigCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.Binary =  DecodeBool(initialFrame.Content, RequestBinaryFieldOffset);
            request.BackupCount =  DecodeInt(initialFrame.Content, RequestBackupCountFieldOffset);
            request.AsyncBackupCount =  DecodeInt(initialFrame.Content, RequestAsyncBackupCountFieldOffset);
            request.StatisticsEnabled =  DecodeBool(initialFrame.Content, RequestStatisticsEnabledFieldOffset);
            request.MergeBatchSize =  DecodeInt(initialFrame.Content, RequestMergeBatchSizeFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            request.CollectionType = StringCodec.Decode(ref iterator);
            request.ListenerConfigs = ListMultiFrameCodec.DecodeNullable(ref iterator, ListenerConfigHolderCodec.Decode);
            request.SplitBrainProtectionName = CodecUtil.DecodeNullable(ref iterator, StringCodec.Decode);
            request.MergePolicy = StringCodec.Decode(ref iterator);
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

        public static DynamicConfigAddMultiMapConfigCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            return response;
        }
    }
}