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
    /// Acquires the lock for the specified key for the specified lease time. After the lease time, the lock will be
    /// released. If the lock is not available, then the current thread becomes disabled for thread scheduling
    /// purposes and lies dormant until the lock has been acquired. Scope of the lock is for this map only. The acquired
    /// lock is only for the key in this map.Locks are re-entrant, so if the key is locked N times, then it should be
    /// unlocked N times before another thread can acquire it.
    ///</summary>
    internal static class MultiMapLockServerCodec
    {
        public const int RequestMessageType = 135168; // 0x021000
        public const int ResponseMessageType = 135169; // 0x021001
        private const int RequestThreadIdFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestTtlFieldOffset = RequestThreadIdFieldOffset + BytesExtensions.SizeOfLong;
        private const int RequestReferenceIdFieldOffset = RequestTtlFieldOffset + BytesExtensions.SizeOfLong;
        private const int RequestInitialFrameSize = RequestReferenceIdFieldOffset + BytesExtensions.SizeOfLong;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

        public sealed class RequestParameters
        {

            /// <summary>
            /// Name of the MultiMap
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// The key the Lock
            ///</summary>
            public IData Key { get; set; }

            /// <summary>
            /// The id of the user thread performing the operation. It is used to guarantee that only the lock holder thread (if a lock exists on the entry) can perform the requested operation
            ///</summary>
            public long ThreadId { get; set; }

            /// <summary>
            /// The duration in milliseconds after which this entry shall be deleted. O means infinite.
            ///</summary>
            public long Ttl { get; set; }

            /// <summary>
            /// The client-wide unique id for this request. It is used to make the request idempotent by sending the same reference id during retries.
            ///</summary>
            public long ReferenceId { get; set; }
        }

        public static ClientMessage EncodeRequest(string name, IData key, long threadId, long ttl, long referenceId)
        {
            var clientMessage = new ClientMessage();
            clientMessage.IsRetryable = true;
            clientMessage.OperationName = "MultiMap.Lock";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteLong(RequestThreadIdFieldOffset, threadId);
            initialFrame.Bytes.WriteLong(RequestTtlFieldOffset, ttl);
            initialFrame.Bytes.WriteLong(RequestReferenceIdFieldOffset, referenceId);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.ThreadId = initialFrame.Bytes.ReadLong(RequestThreadIdFieldOffset);
            request.Ttl = initialFrame.Bytes.ReadLong(RequestTtlFieldOffset);
            request.ReferenceId = initialFrame.Bytes.ReadLong(RequestReferenceIdFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            request.Key = DataCodec.Decode(iterator);
            return request;
        }

        public sealed class ResponseParameters
        {
        }

        public static ClientMessage EncodeResponse()
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Take();
            return response;
        }


    }
}
