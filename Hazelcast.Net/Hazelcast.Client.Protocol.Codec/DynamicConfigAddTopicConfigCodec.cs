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
    /// Adds a new topic configuration to a running cluster.
    /// If a topic configuration with the given {@code name} already exists, then
    /// the new configuration is ignored and the existing one is preserved.
    ///</summary>
    internal static class DynamicConfigAddTopicConfigCodec 
    {
        public const int RequestMessageType = 0x1E0800;
        public const int ResponseMessageType = 0x1E0801;
        private const int RequestGlobalOrderingEnabledFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestStatisticsEnabledFieldOffset = RequestglobalOrderingEnabledFieldOffset + BooleanSizeInBytes;
        private const int RequestMultiThreadingEnabledFieldOffset = RequeststatisticsEnabledFieldOffset + BooleanSizeInBytes;
        private const int RequestInitialFrameSize = RequestMultiThreadingEnabledFieldOffset + BooleanSizeInBytes;
        private const int ResponseInitialFrameSize = CorrelationIdFieldOffset + LongSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// topic's name
            ///</summary>
            public string Name;

            /// <summary>
            /// when {@code true} all nodes listening to the same topic get their messages in
            /// the same order
            ///</summary>
            public bool GlobalOrderingEnabled;

            /// <summary>
            /// {@code true} to enable gathering of statistics, otherwise {@code false}
            ///</summary>
            public bool StatisticsEnabled;

            /// <summary>
            /// {@code true} to enable multi-threaded processing of incoming messages, otherwise
            /// a single thread will handle all topic messages
            ///</summary>
            public bool MultiThreadingEnabled;

            /// <summary>
            /// message listener configurations
            ///</summary>
            public IEnumerable<com.hazelcast.client.impl.protocol.task.dynamicconfig.ListenerConfigHolder> ListenerConfigs;
        }

        public static ClientMessage EncodeRequest(string name, bool globalOrderingEnabled, bool statisticsEnabled, bool multiThreadingEnabled, IEnumerable<com.hazelcast.client.impl.protocol.task.dynamicconfig.ListenerConfigHolder> listenerConfigs) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "DynamicConfig.AddTopicConfig";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeBool(initialFrame.Content, RequestGlobalOrderingEnabledFieldOffset, globalOrderingEnabled);
            EncodeBool(initialFrame.Content, RequestStatisticsEnabledFieldOffset, statisticsEnabled);
            EncodeBool(initialFrame.Content, RequestMultiThreadingEnabledFieldOffset, multiThreadingEnabled);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            ListMultiFrameCodec.EncodeNullable(clientMessage, listenerConfigs, ListenerConfigHolderCodec.Encode);
            return clientMessage;
        }

        public static DynamicConfigAddTopicConfigCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.GlobalOrderingEnabled =  DecodeBool(initialFrame.Content, RequestGlobalOrderingEnabledFieldOffset);
            request.StatisticsEnabled =  DecodeBool(initialFrame.Content, RequestStatisticsEnabledFieldOffset);
            request.MultiThreadingEnabled =  DecodeBool(initialFrame.Content, RequestMultiThreadingEnabledFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            request.ListenerConfigs = ListMultiFrameCodec.DecodeNullable(ref iterator, ListenerConfigHolderCodec.Decode);
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

        public static DynamicConfigAddTopicConfigCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            return response;
        }
    }
}