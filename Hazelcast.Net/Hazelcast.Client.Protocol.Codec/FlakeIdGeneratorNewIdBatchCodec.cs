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
    /// TODO DOC
    ///</summary>
    internal static class FlakeIdGeneratorNewIdBatchCodec 
    {
        //hex: 0x1C0100
        public const int RequestMessageType = 1835264;
        //hex: 0x1C0101
        public const int ResponseMessageType = 1835265;
        private const int RequestBatchSizeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestBatchSizeFieldOffset + IntSizeInBytes;
        private const int ResponseBaseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseIncrementFieldOffset = ResponseBaseFieldOffset + LongSizeInBytes;
        private const int ResponseBatchSizeFieldOffset = ResponseIncrementFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBatchSizeFieldOffset + IntSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// TODO DOC
            ///</summary>
            public string Name;

            /// <summary>
            /// TODO DOC
            ///</summary>
            public int BatchSize;
        }

        public static ClientMessage EncodeRequest(string name, int batchSize) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "FlakeIdGenerator.NewIdBatch";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame.Content, RequestBatchSizeFieldOffset, batchSize);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static FlakeIdGeneratorNewIdBatchCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.BatchSize =  DecodeInt(initialFrame.Content, RequestBatchSizeFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long Base;

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long Increment;

            /// <summary>
            /// TODO DOC
            ///</summary>
            public int BatchSize;
        }

        public static ClientMessage EncodeResponse(long base, long increment, int batchSize) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeLong(initialFrame.Content, ResponseBaseFieldOffset, base);
            EncodeLong(initialFrame.Content, ResponseIncrementFieldOffset, increment);
            EncodeInt(initialFrame.Content, ResponseBatchSizeFieldOffset, batchSize);
            return clientMessage;
        }

        public static FlakeIdGeneratorNewIdBatchCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Base = DecodeLong(initialFrame.Content, ResponseBaseFieldOffset);
            response.Increment = DecodeLong(initialFrame.Content, ResponseIncrementFieldOffset);
            response.BatchSize = DecodeInt(initialFrame.Content, ResponseBatchSizeFieldOffset);
            return response;
        }
    }
}