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
    /// Submits the task to partition for execution, partition is chosen based on multiple criteria of the given task.
    ///</summary>
    internal static class ScheduledExecutorSubmitToPartitionCodec 
    {
        public const int RequestMessageType = 0x1D0200;
        public const int ResponseMessageType = 0x1D0201;
        private const int RequestTypeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialDelayInMillisFieldOffset = RequesttypeFieldOffset + ByteSizeInBytes;
        private const int RequestPeriodInMillisFieldOffset = RequestinitialDelayInMillisFieldOffset + LongSizeInBytes;
        private const int RequestInitialFrameSize = RequestPeriodInMillisFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = CorrelationIdFieldOffset + LongSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// The name of the scheduler.
            ///</summary>
            public string SchedulerName;

            /// <summary>
            /// type of schedule logic, values 0 for SINGLE_RUN, 1 for AT_FIXED_RATE
            ///</summary>
            public byte Type;

            /// <summary>
            /// The name of the task
            ///</summary>
            public string TaskName;

            /// <summary>
            /// Name The name of the task
            ///</summary>
            public IData Task;

            /// <summary>
            /// initial delay in milliseconds
            ///</summary>
            public long InitialDelayInMillis;

            /// <summary>
            /// period between each run in milliseconds
            ///</summary>
            public long PeriodInMillis;
        }

        public static ClientMessage EncodeRequest(string schedulerName, byte type, string taskName, IData task, long initialDelayInMillis, long periodInMillis) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "ScheduledExecutor.SubmitToPartition";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeByte(initialFrame.Content, RequestTypeFieldOffset, type);
            EncodeLong(initialFrame.Content, RequestInitialDelayInMillisFieldOffset, initialDelayInMillis);
            EncodeLong(initialFrame.Content, RequestPeriodInMillisFieldOffset, periodInMillis);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, schedulerName);
            StringCodec.Encode(clientMessage, taskName);
            DataCodec.Encode(clientMessage, task);
            return clientMessage;
        }

        public static ScheduledExecutorSubmitToPartitionCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.Type =  DecodeByte(initialFrame.Content, RequestTypeFieldOffset);
            request.InitialDelayInMillis =  DecodeLong(initialFrame.Content, RequestInitialDelayInMillisFieldOffset);
            request.PeriodInMillis =  DecodeLong(initialFrame.Content, RequestPeriodInMillisFieldOffset);
            request.SchedulerName = StringCodec.Decode(ref iterator);
            request.TaskName = StringCodec.Decode(ref iterator);
            request.Task = DataCodec.Decode(ref iterator);
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

        public static ScheduledExecutorSubmitToPartitionCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            return response;
        }
    }
}