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
    /// Adds the specified entry listener for the specified key.The listener will be notified for all
    /// add/remove/update/evict events for the specified key only.
    ///</summary>
    internal static class MultiMapAddEntryListenerToKeyCodec
    {
        public const int RequestMessageType = 134400; // 0x020D00
        public const int ResponseMessageType = 134401; // 0x020D01
        private const int RequestIncludeValueFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestLocalOnlyFieldOffset = RequestIncludeValueFieldOffset + BytesExtensions.SizeOfBool;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfGuid;
        private const int EventEntryEventTypeFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int EventEntryUuidFieldOffset = EventEntryEventTypeFieldOffset + BytesExtensions.SizeOfInt;
        private const int EventEntryNumberOfAffectedEntriesFieldOffset = EventEntryUuidFieldOffset + BytesExtensions.SizeOfGuid;
        private const int EventEntryInitialFrameSize = EventEntryNumberOfAffectedEntriesFieldOffset + BytesExtensions.SizeOfInt;
        private const int EventEntryMessageType = 134402; // 0x020D02

        public static ClientMessage EncodeRequest(string name, IData key, bool includeValue, bool localOnly)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "MultiMap.AddEntryListenerToKey"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteBoolL(RequestIncludeValueFieldOffset, includeValue);
            initialFrame.Bytes.WriteBoolL(RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            return clientMessage;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// Returns registration id for the entry listener
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

        public static ValueTask HandleEventAsync(ClientMessage clientMessage, HandleEntryEventAsync handleEntryEventAsync, ILoggerFactory loggerFactory)
        {
            using var iterator = clientMessage.GetEnumerator();
            var messageType = clientMessage.MessageType;
            if (messageType == EventEntryMessageType) {
                var initialFrame = iterator.Take();
                var eventType =  initialFrame.Bytes.ReadIntL(EventEntryEventTypeFieldOffset);
                var uuid =  initialFrame.Bytes.ReadGuidL(EventEntryUuidFieldOffset);
                var numberOfAffectedEntries =  initialFrame.Bytes.ReadIntL(EventEntryNumberOfAffectedEntriesFieldOffset);
                var key = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                var @value = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                var oldValue = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                var mergingValue = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                return handleEntryEventAsync(key, @value, oldValue, mergingValue, eventType, uuid, numberOfAffectedEntries);
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
            return default;
        }

        public delegate ValueTask HandleEntryEventAsync(IData key, IData @value, IData oldValue, IData mergingValue, int eventType, Guid uuid, int numberOfAffectedEntries);
    }
}
