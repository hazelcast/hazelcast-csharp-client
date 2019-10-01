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
    /// Adds a new flake ID generator configuration to a running cluster.
    /// If a flake ID generator configuration for the same name already exists, then
    /// the new configuration is ignored and the existing one is preserved.
    ///</summary>
    internal static class DynamicConfigAddFlakeIdGeneratorConfigCodec 
    {
        public const int RequestMessageType = 0x1E1200;
        public const int ResponseMessageType = 0x1E1201;
        private const int RequestPrefetchCountFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestPrefetchValidityFieldOffset = RequestprefetchCountFieldOffset + IntSizeInBytes;
        private const int RequestIdOffsetFieldOffset = RequestprefetchValidityFieldOffset + LongSizeInBytes;
        private const int RequestStatisticsEnabledFieldOffset = RequestidOffsetFieldOffset + LongSizeInBytes;
        private const int RequestNodeIdOffsetFieldOffset = RequeststatisticsEnabledFieldOffset + BooleanSizeInBytes;
        private const int RequestInitialFrameSize = RequestNodeIdOffsetFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = CorrelationIdFieldOffset + LongSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// name of {@code FlakeIdGenerator}
            ///</summary>
            public string Name;

            /// <summary>
            /// how many IDs are pre-fetched on the background when one call to {@code newId()} is made
            ///</summary>
            public int PrefetchCount;

            /// <summary>
            /// for how long the pre-fetched IDs can be used
            ///</summary>
            public long PrefetchValidity;

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long IdOffset;

            /// <summary>
            /// {@code true} to enable gathering of statistics, otherwise {@code false}
            ///</summary>
            public bool StatisticsEnabled;

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long NodeIdOffset;
        }

        public static ClientMessage EncodeRequest(string name, int prefetchCount, long prefetchValidity, long idOffset, bool statisticsEnabled, long nodeIdOffset) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "DynamicConfig.AddFlakeIdGeneratorConfig";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame.Content, RequestPrefetchCountFieldOffset, prefetchCount);
            EncodeLong(initialFrame.Content, RequestPrefetchValidityFieldOffset, prefetchValidity);
            EncodeLong(initialFrame.Content, RequestIdOffsetFieldOffset, idOffset);
            EncodeBool(initialFrame.Content, RequestStatisticsEnabledFieldOffset, statisticsEnabled);
            EncodeLong(initialFrame.Content, RequestNodeIdOffsetFieldOffset, nodeIdOffset);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static DynamicConfigAddFlakeIdGeneratorConfigCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.PrefetchCount =  DecodeInt(initialFrame.Content, RequestPrefetchCountFieldOffset);
            request.PrefetchValidity =  DecodeLong(initialFrame.Content, RequestPrefetchValidityFieldOffset);
            request.IdOffset =  DecodeLong(initialFrame.Content, RequestIdOffsetFieldOffset);
            request.StatisticsEnabled =  DecodeBool(initialFrame.Content, RequestStatisticsEnabledFieldOffset);
            request.NodeIdOffset =  DecodeLong(initialFrame.Content, RequestNodeIdOffsetFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
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

        public static DynamicConfigAddFlakeIdGeneratorConfigCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            return response;
        }
    }
}