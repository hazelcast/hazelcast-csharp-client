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
    /// Associates the specified value with the specified key in this map. If the map previously contained a mapping for
    /// the key, the old value is replaced by the specified value. This method is preferred to #put(Object, Object)
    /// if the old value is not needed.
    /// The object to be set will be accessible only in the current transaction context until the transaction is committed.
    ///</summary>
    internal static class TransactionalMapSetCodec
    {
        //hex: 0x0E0700
        public const int RequestMessageType = 919296;
        //hex: 0x0E0701
        public const int ResponseMessageType = 919297;
        private const int RequestTxnIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestThreadIdFieldOffset = RequestTxnIdFieldOffset + GuidSizeInBytes;
        private const int RequestInitialFrameSize = RequestThreadIdFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + IntSizeInBytes;

        public class RequestParameters
        {

            /// <summary>
            /// Name of the Transactional Map
            ///</summary>
            public string Name;

            /// <summary>
            /// ID of the this transaction operation
            ///</summary>
            public Guid TxnId;

            /// <summary>
            /// The id of the user thread performing the operation. It is used to guarantee that only the lock holder thread (if a lock exists on the entry) can perform the requested operation.
            ///</summary>
            public long ThreadId;

            /// <summary>
            /// The specified key
            ///</summary>
            public IData Key;

            /// <summary>
            /// The value to associate with key
            ///</summary>
            public IData Value;
        }

        public static ClientMessage EncodeRequest(string name, Guid txnId, long threadId, IData key, IData value)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "TransactionalMap.Set";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeGuid(initialFrame.Content, RequestTxnIdFieldOffset, txnId);
            EncodeLong(initialFrame.Content, RequestThreadIdFieldOffset, threadId);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            DataCodec.Encode(clientMessage, value);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.TxnId =  DecodeGuid(initialFrame.Content, RequestTxnIdFieldOffset);
            request.ThreadId =  DecodeLong(initialFrame.Content, RequestThreadIdFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            request.Key = DataCodec.Decode(ref iterator);
            request.Value = DataCodec.Decode(ref iterator);
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