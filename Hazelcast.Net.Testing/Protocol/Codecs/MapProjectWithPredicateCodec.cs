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

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Applies the projection logic on map entries filtered with the Predicate and returns the result
    ///</summary>
    internal static class MapProjectWithPredicateServerCodec
    {
        public const int RequestMessageType = 80896; // 0x013C00
        public const int ResponseMessageType = 80897; // 0x013C01
        private const int RequestInitialFrameSize = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

        public sealed class RequestParameters
        {

            /// <summary>
            /// Name of the map.
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// projection to transform the entries with. May return null.
            ///</summary>
            public IData Projection { get; set; }

            /// <summary>
            /// predicate to filter the entries with
            ///</summary>
            public IData Predicate { get; set; }
        }
    
        public static ClientMessage EncodeRequest(string name, IData projection, IData predicate)
        {
            var clientMessage = new ClientMessage();
            clientMessage.IsRetryable = true;
            clientMessage.OperationName = "Map.ProjectWithPredicate";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.PartitionId, -1);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, projection);
            DataCodec.Encode(clientMessage, predicate);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Take();
            request.Name = StringCodec.Decode(iterator);
            request.Projection = DataCodec.Decode(iterator);
            request.Predicate = DataCodec.Decode(iterator);
            return request;
        }
        
        public sealed class ResponseParameters
        {

            /// <summary>
            /// the resulted collection upon transformation to the type of the projection
            ///</summary>
            public IList<IData> Response { get; set; }
        }

        public static ClientMessage EncodeResponse(ICollection<IData> response)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            clientMessage.Append(initialFrame);
            ListMultiFrameCodec.EncodeContainsNullable(clientMessage, response, DataCodec.Encode);
            return clientMessage;
        }
    
        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Take();
            response.Response = ListMultiFrameCodec.DecodeContainsNullable(iterator, DataCodec.Decode);
            return response;
        }

    
    }
}