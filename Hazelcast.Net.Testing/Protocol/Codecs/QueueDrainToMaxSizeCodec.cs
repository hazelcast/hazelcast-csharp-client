// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

// <auto-generated>
//   This code was generated by a tool.
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantUsingDirective
// ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Logging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using static Hazelcast.Messaging.Portability;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Removes at most the given number of available elements from this queue and adds them to the given collection.
    /// A failure encountered while attempting to add elements to collection may result in elements being in neither,
    /// either or both collections when the associated exception is thrown. Attempts to drain a queue to itself result in
    /// ILLEGAL_ARGUMENT. Further, the behavior of this operation is undefined if the specified collection is
    /// modified while the operation is in progress.
    ///</summary>
    internal static class QueueDrainToMaxSizeServerCodec
    {
        public const int RequestMessageType = 199168; // 0x030A00
        public const int ResponseMessageType = 199169; // 0x030A01
        private const int RequestMaxSizeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestMaxSizeFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + ByteSizeInBytes;

        public sealed class RequestParameters
        {

            /// <summary>
            /// Name of the Queue
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// The maximum number of elements to transfer
            ///</summary>
            public int MaxSize { get; set; }
        }

        public static ClientMessage EncodeRequest(string name, int maxSize)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Queue.DrainToMaxSize";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeInt(initialFrame, RequestMaxSizeFieldOffset, maxSize);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.MaxSize = DecodeInt(initialFrame, RequestMaxSizeFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            return request;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// list of all removed data in result of this method
            ///</summary>
            public IList<IData> Response { get; set; }
        }

        public static ClientMessage EncodeResponse(ICollection<IData> response)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);
            ListMultiFrameCodec.Encode(clientMessage, response, DataCodec.Encode);
            return clientMessage;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Take();
            response.Response = ListMultiFrameCodec.Decode(iterator, DataCodec.Decode);
            return response;
        }


    }
}