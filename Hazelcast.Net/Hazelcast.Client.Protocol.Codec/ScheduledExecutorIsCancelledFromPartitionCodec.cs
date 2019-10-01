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
    /// Checks whether a task as identified from the given handler is already cancelled.
    ///</summary>
    internal static class ScheduledExecutorIsCancelledFromPartitionCodec 
    {
        public const int RequestMessageType = 0x1D0B00;
        public const int ResponseMessageType = 0x1D0B01;
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseResponseFieldOffset = CorrelationIdFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BooleanSizeInBytes;

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
        }

        public static ClientMessage EncodeRequest(string schedulerName, string taskName) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "ScheduledExecutor.IsCancelledFromPartition";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, schedulerName);
            StringCodec.Encode(clientMessage, taskName);
            return clientMessage;
        }

        public static ScheduledExecutorIsCancelledFromPartitionCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Next();
            request.SchedulerName = StringCodec.Decode(ref iterator);
            request.TaskName = StringCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

             /// <summary>
            /// True if the task is cancelled
            ///</summary>
            public bool Response;
        }

        public static ClientMessage EncodeResponse(bool response) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeBool(initialFrame.Content, ResponseResponseFieldOffset, response);
            return clientMessage;
        }

        public static ScheduledExecutorIsCancelledFromPartitionCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeBool(initialFrame.Content, ResponseResponseFieldOffset);
            return response;
        }
    }
}