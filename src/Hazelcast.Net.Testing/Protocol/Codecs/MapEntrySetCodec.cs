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
    /// Returns a Set clone of the mappings contained in this map.
    /// The collection is NOT backed by the map, so changes to the map are NOT reflected in the collection, and vice-versa.
    /// This method is always executed by a distributed query, so it may throw a QueryResultSizeExceededException
    /// if query result size limit is configured.
    ///</summary>
    internal static class MapEntrySetServerCodec
    {
        public const int RequestMessageType = 75008; // 0x012500
        public const int ResponseMessageType = 75009; // 0x012501
        private const int RequestInitialFrameSize = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

        public sealed class RequestParameters
        {

            /// <summary>
            /// name of map
            ///</summary>
            public string Name { get; set; }
        }
    
        public static ClientMessage EncodeRequest(string name)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = true,
                OperationName = "Map.EntrySet"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            iterator.Take(); // empty initial frame
            request.Name = StringCodec.Decode(iterator);
            return request;
        }
        
        public sealed class ResponseParameters
        {

            /// <summary>
            /// a set clone of the keys mappings in this map
            ///</summary>
            public IList<KeyValuePair<IData, IData>> Response { get; set; }
        }

        public static ClientMessage EncodeResponse(ICollection<KeyValuePair<IData, IData>> response)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            clientMessage.Append(initialFrame);
            EntryListCodec.Encode(clientMessage, response, DataCodec.Encode, DataCodec.Encode);
            return clientMessage;
        }
    
        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            
            iterator.Take(); // empty initial frame
            response.Response = EntryListCodec.Decode(iterator, DataCodec.Decode, DataCodec.Decode);
            return response;
        }

    
    }
}