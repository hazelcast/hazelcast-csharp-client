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

using System;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Messaging;
using Hazelcast.Logging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using Hazelcast.Protocol.Portability;
using static Hazelcast.Protocol.Portability.Temp;

// <auto-generated>
//   This code was generated by a tool.
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Acquires the lock for the specified key for the specified lease time. After the lease time, the lock will be
    /// released. If the lock is not available, then the current thread becomes disabled for thread scheduling
    /// purposes and lies dormant until the lock has been acquired. Scope of the lock is for this map only. The acquired
    /// lock is only for the key in this map.Locks are re-entrant, so if the key is locked N times, then it should be
    /// unlocked N times before another thread can acquire it.
    ///</summary>
    internal static class MultiMapLockCodec
    {
        public const int RequestMessageType = 135168; // 0x021000
        public const int ResponseMessageType = 135169; // 0x021001
        private const int RequestThreadIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestTtlFieldOffset = RequestThreadIdFieldOffset + LongSizeInBytes;
        private const int RequestReferenceIdFieldOffset = RequestTtlFieldOffset + LongSizeInBytes;
        private const int RequestInitialFrameSize = RequestReferenceIdFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + ByteSizeInBytes;

        public static ClientMessage EncodeRequest(string name, IData key, long threadId, long ttl, long referenceId)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.OperationName = "MultiMap.Lock";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeLong(initialFrame, RequestThreadIdFieldOffset, threadId);
            EncodeLong(initialFrame, RequestTtlFieldOffset, ttl);
            EncodeLong(initialFrame, RequestReferenceIdFieldOffset, referenceId);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            return clientMessage;
        }

        public class ResponseParameters
        {
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members