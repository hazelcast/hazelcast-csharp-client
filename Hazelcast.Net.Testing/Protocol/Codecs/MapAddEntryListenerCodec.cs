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
    /// Adds a MapListener for this map. To receive an event, you should implement a corresponding MapListener
    /// sub-interface for that event.
    ///</summary>
    internal static class MapAddEntryListenerServerCodec
    {
        public const int RequestMessageType = 71936; // 0x011900
        public const int ResponseMessageType = 71937; // 0x011901
        private const int RequestIncludeValueFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestListenerFlagsFieldOffset = RequestIncludeValueFieldOffset + BoolSizeInBytes;
        private const int RequestLocalOnlyFieldOffset = RequestListenerFlagsFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EntryEventEventTypeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EntryEventUuidFieldOffset = EntryEventEventTypeFieldOffset + IntSizeInBytes;
        private const int EntryEventNumberOfAffectedEntriesFieldOffset = EntryEventUuidFieldOffset + GuidSizeInBytes;
        private const int EntryEventInitialFrameSize = EntryEventNumberOfAffectedEntriesFieldOffset + IntSizeInBytes;
        private const int EntryEventMessageType = 71938; // 0x011902

        public sealed class RequestParameters
        {

            /// <summary>
            /// name of map
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// true if EntryEvent should contain the value.
            ///</summary>
            public bool IncludeValue { get; set; }

            /// <summary>
            /// flags of enabled listeners.
            ///</summary>
            public int ListenerFlags { get; set; }

            /// <summary>
            /// if true fires events that originated from this node only, otherwise fires all events
            ///</summary>
            public bool LocalOnly { get; set; }
        }

        public static ClientMessage EncodeRequest(string name, bool includeValue, int listenerFlags, bool localOnly)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Map.AddEntryListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeBool(initialFrame, RequestIncludeValueFieldOffset, includeValue);
            EncodeInt(initialFrame, RequestListenerFlagsFieldOffset, listenerFlags);
            EncodeBool(initialFrame, RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.IncludeValue = DecodeBool(initialFrame, RequestIncludeValueFieldOffset);
            request.ListenerFlags = DecodeInt(initialFrame, RequestListenerFlagsFieldOffset);
            request.LocalOnly = DecodeBool(initialFrame, RequestLocalOnlyFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            return request;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// A unique string which is used as a key to remove the listener.
            ///</summary>
            public Guid Response { get; set; }
        }

        public static ClientMessage EncodeResponse(Guid response)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, ResponseMessageType);
            EncodeGuid(initialFrame, ResponseResponseFieldOffset, response);
            clientMessage.Add(initialFrame);
            return clientMessage;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = DecodeGuid(initialFrame, ResponseResponseFieldOffset);
            return response;
        }

        public static ClientMessage EncodeEntryEvent(IData key, IData @value, IData oldValue, IData mergingValue, int eventType, Guid uuid, int numberOfAffectedEntries)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EntryEventInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, EntryEventMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeInt(initialFrame, EntryEventEventTypeFieldOffset, eventType);
            EncodeGuid(initialFrame, EntryEventUuidFieldOffset, uuid);
            EncodeInt(initialFrame, EntryEventNumberOfAffectedEntriesFieldOffset, numberOfAffectedEntries);
            clientMessage.Add(initialFrame);
            clientMessage.Flags |= ClientMessageFlags.Event;
            CodecUtil.EncodeNullable(clientMessage, key, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, @value, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, oldValue, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, mergingValue, DataCodec.Encode);
            return clientMessage;
        }

        public static void HandleEvent(ClientMessage clientMessage, HandleEntryEvent handleEntryEvent, ILoggerFactory loggerFactory)
        {
            var messageType = clientMessage.MessageType;
            var iterator = clientMessage.GetIterator();
            if (messageType == EntryEventMessageType) {
                var initialFrame = iterator.Take();
                var eventType =  DecodeInt(initialFrame, EntryEventEventTypeFieldOffset);
                var uuid =  DecodeGuid(initialFrame, EntryEventUuidFieldOffset);
                var numberOfAffectedEntries =  DecodeInt(initialFrame, EntryEventNumberOfAffectedEntriesFieldOffset);
                var key = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                var @value = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                var oldValue = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                var mergingValue = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                handleEntryEvent(key, @value, oldValue, mergingValue, eventType, uuid, numberOfAffectedEntries);
                return;
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
        }

        public delegate void HandleEntryEvent(IData key, IData @value, IData oldValue, IData mergingValue, int eventType, Guid uuid, int numberOfAffectedEntries);
    }
}