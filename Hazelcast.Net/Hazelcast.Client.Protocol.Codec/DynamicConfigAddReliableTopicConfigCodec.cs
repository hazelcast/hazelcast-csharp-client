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
    /// Adds a new reliable topic configuration to a running cluster.
    /// If a reliable topic configuration with the given {@code name} already exists, then
    /// the new configuration is ignored and the existing one is preserved.
    ///</summary>
    internal static class DynamicConfigAddReliableTopicConfigCodec 
    {
        //hex: 0x1E0F00
        public const int RequestMessageType = 1969920;
        //hex: 0x1E0F01
        public const int ResponseMessageType = 1969921;
        private const int RequestReadBatchSizeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestStatisticsEnabledFieldOffset = RequestReadBatchSizeFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestStatisticsEnabledFieldOffset + BoolSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + IntSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// name of reliable topic
            ///</summary>
            public string Name;

            /// <summary>
            /// message listener configurations
            ///</summary>
            public IEnumerable<com.hazelcast.client.impl.protocol.task.dynamicconfig.ListenerConfigHolder> ListenerConfigs;

            /// <summary>
            /// maximum number of items to read in a batch.
            ///</summary>
            public int ReadBatchSize;

            /// <summary>
            /// {@code true} to enable gathering of statistics, otherwise {@code false}
            ///</summary>
            public bool StatisticsEnabled;

            /// <summary>
            /// policy to handle an overloaded topic. Available values are {@code DISCARD_OLDEST},
            /// {@code DISCARD_NEWEST}, {@code BLOCK} and {@code ERROR}.
            ///</summary>
            public string TopicOverloadPolicy;

            /// <summary>
            /// a serialized {@link java.util.concurrent.Executor} instance to use for executing
            /// message listeners or {@code null}
            ///</summary>
            public IData Executor;
        }

        public static ClientMessage EncodeRequest(string name, IEnumerable<com.hazelcast.client.impl.protocol.task.dynamicconfig.ListenerConfigHolder> listenerConfigs, int readBatchSize, bool statisticsEnabled, string topicOverloadPolicy, IData executor) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "DynamicConfig.AddReliableTopicConfig";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame.Content, RequestReadBatchSizeFieldOffset, readBatchSize);
            EncodeBool(initialFrame.Content, RequestStatisticsEnabledFieldOffset, statisticsEnabled);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            ListMultiFrameCodec.EncodeNullable(clientMessage, listenerConfigs, ListenerConfigHolderCodec.Encode);
            StringCodec.Encode(clientMessage, topicOverloadPolicy);
            CodecUtil.EncodeNullable(clientMessage, executor, DataCodec.Encode);
            return clientMessage;
        }

        public static DynamicConfigAddReliableTopicConfigCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.ReadBatchSize =  DecodeInt(initialFrame.Content, RequestReadBatchSizeFieldOffset);
            request.StatisticsEnabled =  DecodeBool(initialFrame.Content, RequestStatisticsEnabledFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            request.ListenerConfigs = ListMultiFrameCodec.DecodeNullable(ref iterator, ListenerConfigHolderCodec.Decode);
            request.TopicOverloadPolicy = StringCodec.Decode(ref iterator);
            request.Executor = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
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

        public static DynamicConfigAddReliableTopicConfigCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            return response;
        }
    }
}