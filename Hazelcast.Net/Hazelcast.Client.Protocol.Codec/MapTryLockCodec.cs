// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Codec.Custom;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol.Codec
{
    // This file is auto-generated by the Hazelcast Client Protocol Code Generator.
    // To change this file, edit the templates or the protocol
    // definitions on the https://github.com/hazelcast/hazelcast-client-protocol
    // and regenerate it.

    /// <summary>
    /// Tries to acquire the lock for the specified key for the specified lease time.After lease time, the lock will be
    /// released.If the lock is not available, then the current thread becomes disabled for thread scheduling
    /// purposes and lies dormant until one of two things happens the lock is acquired by the current thread, or
    /// the specified waiting time elapses.
    ///</summary>
    internal static class MapTryLockCodec 
    {
        //hex: 0x011400
        public const int RequestMessageType = 70656;
        //hex: 0x011401
        public const int ResponseMessageType = 70657;
        private const int RequestThreadIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestLeaseFieldOffset = RequestThreadIdFieldOffset + LongSizeInBytes;
        private const int RequestTimeoutFieldOffset = RequestLeaseFieldOffset + LongSizeInBytes;
        private const int RequestReferenceIdFieldOffset = RequestTimeoutFieldOffset + LongSizeInBytes;
        private const int RequestInitialFrameSize = RequestReferenceIdFieldOffset + LongSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BoolSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the map.
            ///</summary>
            public string Name;

            /// <summary>
            /// Key for the map entry.
            ///</summary>
            public IData Key;

            /// <summary>
            /// The id of the user thread performing the operation. It is used to guarantee that only the lock holder thread (if a lock exists on the entry) can perform the requested operation.
            ///</summary>
            public long ThreadId;

            /// <summary>
            /// time in milliseconds to wait before releasing the lock.
            ///</summary>
            public long Lease;

            /// <summary>
            /// maximum time to wait for getting the lock.
            ///</summary>
            public long Timeout;

            /// <summary>
            /// The client-wide unique id for this request. It is used to make the request idempotent by sending the same reference id during retries.
            ///</summary>
            public long ReferenceId;
        }

        public static ClientMessage EncodeRequest(string name, IData key, long threadId, long lease, long timeout, long referenceId) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = true;
            clientMessage.OperationName = "Map.TryLock";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeLong(initialFrame.Content, RequestThreadIdFieldOffset, threadId);
            EncodeLong(initialFrame.Content, RequestLeaseFieldOffset, lease);
            EncodeLong(initialFrame.Content, RequestTimeoutFieldOffset, timeout);
            EncodeLong(initialFrame.Content, RequestReferenceIdFieldOffset, referenceId);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            return clientMessage;
        }

        public static MapTryLockCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.ThreadId =  DecodeLong(initialFrame.Content, RequestThreadIdFieldOffset);
            request.Lease =  DecodeLong(initialFrame.Content, RequestLeaseFieldOffset);
            request.Timeout =  DecodeLong(initialFrame.Content, RequestTimeoutFieldOffset);
            request.ReferenceId =  DecodeLong(initialFrame.Content, RequestReferenceIdFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            request.Key = DataCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// Returns true if successful, otherwise returns false
            ///</summary>
            public bool Response;
        }

        public static ClientMessage EncodeResponse(bool response) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeBool(initialFrame.Content, ResponseResponseFieldOffset, response);
            return clientMessage;
        }

        public static MapTryLockCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeBool(initialFrame.Content, ResponseResponseFieldOffset);
            return response;
        }
    }
}