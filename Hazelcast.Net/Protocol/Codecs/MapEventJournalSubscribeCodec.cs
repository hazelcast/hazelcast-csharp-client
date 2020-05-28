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
    /// Performs the initial subscription to the map event journal.
    /// This includes retrieving the event journal sequences of the
    /// oldest and newest event in the journal.
    ///</summary>
    internal static class MapEventJournalSubscribeCodec
    {
        public const int RequestMessageType = 82176; // 0x014100
        public const int ResponseMessageType = 82177; // 0x014101
        private const int RequestInitialFrameSize = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int ResponseOldestSequenceFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseNewestSequenceFieldOffset = ResponseOldestSequenceFieldOffset + BytesExtensions.SizeOfLong;
        private const int ResponseInitialFrameSize = ResponseNewestSequenceFieldOffset + BytesExtensions.SizeOfLong;

        public static ClientMessage EncodeRequest(string name)
        {
            var clientMessage = new ClientMessage();
            clientMessage.IsRetryable = true;
            clientMessage.OperationName = "Map.EventJournalSubscribe";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.PartitionId, -1);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// Sequence id of the oldest event in the event journal.
            ///</summary>
            public long OldestSequence { get; set; }

            /// <summary>
            /// Sequence id of the newest event in the event journal.
            ///</summary>
            public long NewestSequence { get; set; }
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.OldestSequence = initialFrame.Bytes.ReadLong(ResponseOldestSequenceFieldOffset);
            response.NewestSequence = initialFrame.Bytes.ReadLong(ResponseNewestSequenceFieldOffset);
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members