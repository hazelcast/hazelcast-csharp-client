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
    /// Removes all the entries with the given key. The collection is NOT backed by the map, so changes to the map are
    /// NOT reflected in the collection, and vice-versa.
    ///</summary>
    internal static class MultiMapRemoveEntryServerCodec
    {
        public const int RequestMessageType = 136448; // 0x021500
        public const int ResponseMessageType = 136449; // 0x021501
        private const int RequestThreadIdFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestThreadIdFieldOffset + BytesExtensions.SizeOfLong;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfBool;

        public sealed class RequestParameters
        {

            /// <summary>
            /// Name of the MultiMap
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// The key of the entry to remove
            ///</summary>
            public IData Key { get; set; }

            /// <summary>
            /// The value of the entry to remove
            ///</summary>
            public IData Value { get; set; }

            /// <summary>
            /// The id of the user thread performing the operation. It is used to guarantee that only the lock holder thread (if a lock exists on the entry) can perform the requested operation
            ///</summary>
            public long ThreadId { get; set; }
        }
    
        public static ClientMessage EncodeRequest(string name, IData key, IData @value, long threadId)
        {
            var clientMessage = new ClientMessage();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "MultiMap.RemoveEntry";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteLong(RequestThreadIdFieldOffset, threadId);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            DataCodec.Encode(clientMessage, @value);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.ThreadId = initialFrame.Bytes.ReadLong(RequestThreadIdFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            request.Key = DataCodec.Decode(iterator);
            request.Value = DataCodec.Decode(iterator);
            return request;
        }
        
        public sealed class ResponseParameters
        {

            /// <summary>
            /// True if the size of the multimap changed after the remove operation, false otherwise.
            ///</summary>
            public bool Response { get; set; }
        }

        public static ClientMessage EncodeResponse(bool response)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            initialFrame.Bytes.WriteBool(ResponseResponseFieldOffset, response);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }
    
        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = initialFrame.Bytes.ReadBool(ResponseResponseFieldOffset);
            return response;
        }

    
    }
}