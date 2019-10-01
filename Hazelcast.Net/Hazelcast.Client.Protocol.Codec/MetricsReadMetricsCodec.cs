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
    /// Reads the recorded metrics starting with the smallest sequence number
    /// greater or equals to the sequence number set in fromSequence.
    ///</summary>
    internal static class MetricsReadMetricsCodec 
    {
        public const int RequestMessageType = 0x270100;
        public const int ResponseMessageType = 0x270101;
        private const int RequestUuidFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestFromSequenceFieldOffset = RequestuuidFieldOffset + UUIDSizeInBytes;
        private const int RequestInitialFrameSize = RequestFromSequenceFieldOffset + LongSizeInBytes;
        private const int ResponseNextSequenceFieldOffset = CorrelationIdFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseNextSequenceFieldOffset + LongSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// The UUID of the member that is supposed to read the metrics from.
            ///</summary>
            public Guid Uuid;

            /// <summary>
            /// The sequence the recorded metrics should be read starting with.
            ///</summary>
            public long FromSequence;
        }

        public static ClientMessage EncodeRequest(Guid uuid, long fromSequence) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Metrics.ReadMetrics";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeGuid(initialFrame.Content, RequestUuidFieldOffset, uuid);
            EncodeLong(initialFrame.Content, RequestFromSequenceFieldOffset, fromSequence);
            clientMessage.Add(initialFrame);
            return clientMessage;
        }

        public static MetricsReadMetricsCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.Uuid =  DecodeGuid(initialFrame.Content, RequestUuidFieldOffset);
            request.FromSequence =  DecodeLong(initialFrame.Content, RequestFromSequenceFieldOffset);
            return request;
        }

        public class ResponseParameters 
        {

             /// <summary>
            /// The map of timestamp and compressed metrics data
            ///</summary>
            public IEnumerable<KeyValuePair<long, byte[]>> Elements;

             /// <summary>
            /// The sequence number that the next task should start with
            ///</summary>
            public long NextSequence;
        }

        public static ClientMessage EncodeResponse(IEnumerable<KeyValuePair<long, byte[]>> elements, long nextSequence) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeLong(initialFrame.Content, ResponseNextSequenceFieldOffset, nextSequence);
            EntryListLongByteArrayCodec.Encode(clientMessage, elements);
            return clientMessage;
        }

        public static MetricsReadMetricsCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.NextSequence = DecodeLong(initialFrame.Content, ResponseNextSequenceFieldOffset);
            response.Elements = EntryListLongByteArrayCodec.Decode(ref iterator);
            return response;
        }
    }
}