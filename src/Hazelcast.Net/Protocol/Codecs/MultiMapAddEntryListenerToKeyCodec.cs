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
    /// Adds the specified entry listener for the specified key.The listener will be notified for all
    /// add/remove/update/evict events for the specified key only.
    ///</summary>
#if SERVER_CODEC
    internal static class MultiMapAddEntryListenerToKeyServerCodec
#else
    internal static class MultiMapAddEntryListenerToKeyCodec
#endif
    {
        public const int RequestMessageType = 134400; // 0x020D00
        public const int ResponseMessageType = 134401; // 0x020D01
        private const int RequestIncludeValueFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestLocalOnlyFieldOffset = RequestIncludeValueFieldOffset + BytesExtensions.SizeOfBool;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfCodecGuid;
        private const int EventEntryEventTypeFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int EventEntryUuidFieldOffset = EventEntryEventTypeFieldOffset + BytesExtensions.SizeOfInt;
        private const int EventEntryNumberOfAffectedEntriesFieldOffset = EventEntryUuidFieldOffset + BytesExtensions.SizeOfCodecGuid;
        private const int EventEntryInitialFrameSize = EventEntryNumberOfAffectedEntriesFieldOffset + BytesExtensions.SizeOfInt;
        private const int EventEntryMessageType = 134402; // 0x020D02

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// Name of the MultiMap
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// The key to listen to
            ///</summary>
            public IData Key { get; set; }

            /// <summary>
            /// True if EntryEvent should contain the value,false otherwise
            ///</summary>
            public bool IncludeValue { get; set; }

            /// <summary>
            /// if true fires events that originated from this node only, otherwise fires all events
            ///</summary>
            public bool LocalOnly { get; set; }
        }
#endif

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

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.IncludeValue = initialFrame.Bytes.ReadBoolL(RequestIncludeValueFieldOffset);
            request.LocalOnly = initialFrame.Bytes.ReadBoolL(RequestLocalOnlyFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            request.Key = DataCodec.Decode(iterator);
            return request;
        }
#endif

        public sealed class ResponseParameters
        {

            /// <summary>
            /// Returns registration id for the entry listener
            ///</summary>
            public Guid Response { get; set; }
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse(Guid response)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            initialFrame.Bytes.WriteGuidL(ResponseResponseFieldOffset, response);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = initialFrame.Bytes.ReadGuidL(ResponseResponseFieldOffset);
            return response;
        }

#if SERVER_CODEC
        public static ClientMessage EncodeEntryEvent(IData key, IData @value, IData oldValue, IData mergingValue, int eventType, Guid uuid, int numberOfAffectedEntries)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[EventEntryInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, EventEntryMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteIntL(EventEntryEventTypeFieldOffset, eventType);
            initialFrame.Bytes.WriteGuidL(EventEntryUuidFieldOffset, uuid);
            initialFrame.Bytes.WriteIntL(EventEntryNumberOfAffectedEntriesFieldOffset, numberOfAffectedEntries);
            clientMessage.Append(initialFrame);
            clientMessage.Flags |= ClientMessageFlags.Event;
            CodecUtil.EncodeNullable(clientMessage, key, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, @value, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, oldValue, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, mergingValue, DataCodec.Encode);
            return clientMessage;
        }
#endif
        public static ValueTask HandleEventAsync(ClientMessage clientMessage, Func<IData, IData, IData, IData, int, Guid, int, object, ValueTask> handleEntryEventAsync, object state, ILoggerFactory loggerFactory)
        {
            using var iterator = clientMessage.GetEnumerator();
            var messageType = clientMessage.MessageType;
            if (messageType == EventEntryMessageType)
            {
                var initialFrame = iterator.Take();
                var eventType =  initialFrame.Bytes.ReadIntL(EventEntryEventTypeFieldOffset);
                var uuid =  initialFrame.Bytes.ReadGuidL(EventEntryUuidFieldOffset);
                var numberOfAffectedEntries =  initialFrame.Bytes.ReadIntL(EventEntryNumberOfAffectedEntriesFieldOffset);
                var key = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                var @value = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                var oldValue = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                var mergingValue = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                return handleEntryEventAsync(key, @value, oldValue, mergingValue, eventType, uuid, numberOfAffectedEntries, state);
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
            return default;
        }
    }
}
