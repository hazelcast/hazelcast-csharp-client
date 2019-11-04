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
    /// Returns statistics of the task
    ///</summary>
    internal static class ScheduledExecutorGetStatsFromAddressCodec 
    {
        //hex: 0x1A0600
        public const int RequestMessageType = 1705472;
        //hex: 0x1A0601
        public const int ResponseMessageType = 1705473;
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseLastIdleTimeNanosFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseTotalIdleTimeNanosFieldOffset = ResponseLastIdleTimeNanosFieldOffset + LongSizeInBytes;
        private const int ResponseTotalRunsFieldOffset = ResponseTotalIdleTimeNanosFieldOffset + LongSizeInBytes;
        private const int ResponseTotalRunTimeNanosFieldOffset = ResponseTotalRunsFieldOffset + LongSizeInBytes;
        private const int ResponseLastRunDurationNanosFieldOffset = ResponseTotalRunTimeNanosFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseLastRunDurationNanosFieldOffset + LongSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// The name of the scheduler.
            ///</summary>
            public string SchedulerName;

            /// <summary>
            /// The name of the task
            ///</summary>
            public string TaskName;

            /// <summary>
            /// The address of the member where the task will get scheduled.
            ///</summary>
            public com.hazelcast.nio.Address Address;
        }

        public static ClientMessage EncodeRequest(string schedulerName, string taskName, com.hazelcast.nio.Address address) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "ScheduledExecutor.GetStatsFromAddress";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, schedulerName);
            StringCodec.Encode(clientMessage, taskName);
            AddressCodec.Encode(clientMessage, address);
            return clientMessage;
        }

        public static ScheduledExecutorGetStatsFromAddressCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Next();
            request.SchedulerName = StringCodec.Decode(ref iterator);
            request.TaskName = StringCodec.Decode(ref iterator);
            request.Address = AddressCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long LastIdleTimeNanos;

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long TotalIdleTimeNanos;

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long TotalRuns;

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long TotalRunTimeNanos;

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long LastRunDurationNanos;
        }

        public static ClientMessage EncodeResponse(long lastIdleTimeNanos, long totalIdleTimeNanos, long totalRuns, long totalRunTimeNanos, long lastRunDurationNanos) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeLong(initialFrame.Content, ResponseLastIdleTimeNanosFieldOffset, lastIdleTimeNanos);
            EncodeLong(initialFrame.Content, ResponseTotalIdleTimeNanosFieldOffset, totalIdleTimeNanos);
            EncodeLong(initialFrame.Content, ResponseTotalRunsFieldOffset, totalRuns);
            EncodeLong(initialFrame.Content, ResponseTotalRunTimeNanosFieldOffset, totalRunTimeNanos);
            EncodeLong(initialFrame.Content, ResponseLastRunDurationNanosFieldOffset, lastRunDurationNanos);
            return clientMessage;
        }

        public static ScheduledExecutorGetStatsFromAddressCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.LastIdleTimeNanos = DecodeLong(initialFrame.Content, ResponseLastIdleTimeNanosFieldOffset);
            response.TotalIdleTimeNanos = DecodeLong(initialFrame.Content, ResponseTotalIdleTimeNanosFieldOffset);
            response.TotalRuns = DecodeLong(initialFrame.Content, ResponseTotalRunsFieldOffset);
            response.TotalRunTimeNanos = DecodeLong(initialFrame.Content, ResponseTotalRunTimeNanosFieldOffset);
            response.LastRunDurationNanos = DecodeLong(initialFrame.Content, ResponseLastRunDurationNanosFieldOffset);
            return response;
        }
    }
}