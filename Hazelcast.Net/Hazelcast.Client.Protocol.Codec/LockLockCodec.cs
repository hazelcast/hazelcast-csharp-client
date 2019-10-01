/*
 * Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol.Codec
{
    /*
    * This file is auto-generated by the Hazelcast Client Protocol Code Generator.
    * To change this file, edit the templates or the protocol
    * definitions on the https://github.com/hazelcast/hazelcast-client-protocol
    * and regenerate it.
    */

    /// <summary>
    /// Acquires the lock for the specified lease time.After lease time, lock will be released.If the lock is not
    /// available then the current thread becomes disabled for thread scheduling purposes and lies dormant until the lock
    /// has been acquired.
    ///</summary>
    internal static class LockLockCodec 
    {
        public const int RequestMessageType = 0x070500;
        public const int ResponseMessageType = 0x070501;
        private const int RequestLeaseTimeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestThreadIdFieldOffset = RequestleaseTimeFieldOffset + LongSizeInBytes;
        private const int RequestReferenceIdFieldOffset = RequestthreadIdFieldOffset + LongSizeInBytes;
        private const int RequestInitialFrameSize = RequestReferenceIdFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = CorrelationIdFieldOffset + LongSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the Lock
            ///</summary>
            public string Name;

            /// <summary>
            /// Time to wait before releasing to lock
            ///</summary>
            public long LeaseTime;

            /// <summary>
            /// The id of the user thread performing the operation. It is used to guarantee that only the lock holder thread (if a lock exists on the entry) can perform the requested operation.
            ///</summary>
            public long ThreadId;

            /// <summary>
            /// The client-wide unique id for this request. It is used to make the request idempotent by sending the same reference id during retries.
            ///</summary>
            public long ReferenceId;
        }

        public static ClientMessage EncodeRequest(string name, long leaseTime, long threadId, long referenceId) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = true;
            clientMessage.OperationName = "Lock.Lock";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeLong(initialFrame.Content, RequestLeaseTimeFieldOffset, leaseTime);
            EncodeLong(initialFrame.Content, RequestThreadIdFieldOffset, threadId);
            EncodeLong(initialFrame.Content, RequestReferenceIdFieldOffset, referenceId);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static LockLockCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.LeaseTime =  DecodeLong(initialFrame.Content, RequestLeaseTimeFieldOffset);
            request.ThreadId =  DecodeLong(initialFrame.Content, RequestThreadIdFieldOffset);
            request.ReferenceId =  DecodeLong(initialFrame.Content, RequestReferenceIdFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {
        }

        public static ClientMessage EncodeResponse() 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            return clientMessage;
        }

        public static LockLockCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            return response;
        }
    }
}