﻿// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
//   Hazelcast Client Protocol Code Generator @0a5719d
//   https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantUsingDirective
// ReSharper disable CheckNamespace

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Copies all of the mappings from the specified map to this map (optional operation).The effect of this call is
    /// equivalent to that of calling put(Object,Object) put(k, v) on this map once for each mapping from key k to value
    /// v in the specified map.The behavior of this operation is undefined if the specified map is modified while the
    /// operation is in progress.
    /// Please note that all the keys in the request should belong to the partition id to which this request is being sent, all keys
    /// matching to a different partition id shall be ignored. The API implementation using this request may need to send multiple
    /// of these request messages for filling a request for a key set if the keys belong to different partitions.
    ///</summary>
#if SERVER_CODEC
    internal static class MapPutAllServerCodec
#else
    internal static class MapPutAllCodec
#endif
    {
        public const int RequestMessageType = 76800; // 0x012C00
        public const int ResponseMessageType = 76801; // 0x012C01
        private const int RequestTriggerMapLoaderFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestTriggerMapLoaderFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// name of map
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// mappings to be stored in this map
            ///</summary>
            public IList<KeyValuePair<IData, IData>> Entries { get; set; }

            /// <summary>
            /// should trigger MapLoader for elements not in this map
            ///</summary>
            public bool TriggerMapLoader { get; set; }

            /// <summary>
            /// <c>true</c> if the triggerMapLoader is received from the client, <c>false</c> otherwise.
            /// If this is false, triggerMapLoader has the default value for its type.
            /// </summary>
            public bool IsTriggerMapLoaderExists { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(string name, ICollection<KeyValuePair<IData, IData>> entries, bool triggerMapLoader)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "Map.PutAll"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteBoolL(RequestTriggerMapLoaderFieldOffset, triggerMapLoader);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            EntryListCodec.Encode(clientMessage, entries, DataCodec.Encode, DataCodec.Encode);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            if (initialFrame.Bytes.Length >= RequestTriggerMapLoaderFieldOffset + BytesExtensions.SizeOfBool)
            {
                request.TriggerMapLoader = initialFrame.Bytes.ReadBoolL(RequestTriggerMapLoaderFieldOffset);
                request.IsTriggerMapLoaderExists = true;
            }
            else request.IsTriggerMapLoaderExists = false;
            request.Name = StringCodec.Decode(iterator);
            request.Entries = EntryListCodec.Decode(iterator, DataCodec.Decode, DataCodec.Decode);
            return request;
        }
#endif

        public sealed class ResponseParameters
        {
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse()
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            iterator.Take(); // empty initial frame
            return response;
        }

    }
}
