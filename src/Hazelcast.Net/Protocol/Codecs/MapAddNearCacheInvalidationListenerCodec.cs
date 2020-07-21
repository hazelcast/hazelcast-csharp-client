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
using System.Threading;
using System.Threading.Tasks;
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
    /// Adds listener to map. This listener will be used to listen near cache invalidation events.
    ///</summary>
    internal static class MapAddNearCacheInvalidationListenerCodec
    {
        public const int RequestMessageType = 81664; // 0x013F00
        public const int ResponseMessageType = 81665; // 0x013F01
        private const int RequestListenerFlagsFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestLocalOnlyFieldOffset = RequestListenerFlagsFieldOffset + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfGuid;
        private const int EventIMapInvalidationSourceUuidFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int EventIMapInvalidationPartitionUuidFieldOffset = EventIMapInvalidationSourceUuidFieldOffset + BytesExtensions.SizeOfGuid;
        private const int EventIMapInvalidationSequenceFieldOffset = EventIMapInvalidationPartitionUuidFieldOffset + BytesExtensions.SizeOfGuid;
        private const int EventIMapInvalidationInitialFrameSize = EventIMapInvalidationSequenceFieldOffset + BytesExtensions.SizeOfLong;
        private const int EventIMapInvalidationMessageType = 81666; // 0x013F02
        private const int EventIMapBatchInvalidationInitialFrameSize = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int EventIMapBatchInvalidationMessageType = 81667; // 0x013F03

        public static ClientMessage EncodeRequest(string name, int listenerFlags, bool localOnly)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "Map.AddNearCacheInvalidationListener"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteIntL(RequestListenerFlagsFieldOffset, listenerFlags);
            initialFrame.Bytes.WriteBoolL(RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// A unique string which is used as a key to remove the listener.
            ///</summary>
            public Guid Response { get; set; }
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = initialFrame.Bytes.ReadGuidL(ResponseResponseFieldOffset);
            return response;
        }

        public static ValueTask HandleEventAsync(ClientMessage clientMessage, HandleIMapInvalidationEventAsync handleIMapInvalidationEventAsync, HandleIMapBatchInvalidationEventAsync handleIMapBatchInvalidationEventAsync, ILoggerFactory loggerFactory)
        {
            using var iterator = clientMessage.GetEnumerator();
            var messageType = clientMessage.MessageType;
            if (messageType == EventIMapInvalidationMessageType) {
                var initialFrame = iterator.Take();
                var sourceUuid =  initialFrame.Bytes.ReadGuidL(EventIMapInvalidationSourceUuidFieldOffset);
                var partitionUuid =  initialFrame.Bytes.ReadGuidL(EventIMapInvalidationPartitionUuidFieldOffset);
                var sequence =  initialFrame.Bytes.ReadLongL(EventIMapInvalidationSequenceFieldOffset);
                var key = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                return handleIMapInvalidationEventAsync(key, sourceUuid, partitionUuid, sequence);
            }
            if (messageType == EventIMapBatchInvalidationMessageType) {
                
                iterator.Take(); // empty initial frame
                var keys = ListMultiFrameCodec.Decode(iterator, DataCodec.Decode);
                var sourceUuids = ListUUIDCodec.Decode(iterator);
                var partitionUuids = ListUUIDCodec.Decode(iterator);
                var sequences = ListLongCodec.Decode(iterator);
                return handleIMapBatchInvalidationEventAsync(keys, sourceUuids, partitionUuids, sequences);
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
            return default;
        }

        public delegate ValueTask HandleIMapInvalidationEventAsync(IData key, Guid sourceUuid, Guid partitionUuid, long sequence);

        public delegate ValueTask HandleIMapBatchInvalidationEventAsync(ICollection<IData> keys, ICollection<Guid> sourceUuids, ICollection<Guid> partitionUuids, ICollection<long> sequences);
    }
}