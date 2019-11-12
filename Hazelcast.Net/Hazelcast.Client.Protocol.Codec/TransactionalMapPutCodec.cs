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
    /// the key, the old value is replaced by the specified value. The object to be put will be accessible only in the
    /// current transaction context till transaction is committed.
    ///</summary>
    internal static class TransactionalMapPutCodec
    {
        //hex: 0x0E0600
        public const int RequestMessageType = 919040;
        //hex: 0x0E0601
        public const int ResponseMessageType = 919041;
        private const int RequestTxnIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestThreadIdFieldOffset = RequestTxnIdFieldOffset + GuidSizeInBytes;
        private const int RequestTtlFieldOffset = RequestThreadIdFieldOffset + LongSizeInBytes;
        private const int RequestInitialFrameSize = RequestTtlFieldOffset + LongSizeInBytes;
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
            /// The value to associate with the key.
            ///</summary>
            public IData Value;

            /// <summary>
            /// The duration in milliseconds after which this entry shall be deleted. O means infinite.
            ///</summary>
            public long Ttl;
        }

        public static ClientMessage EncodeRequest(string name, Guid txnId, long threadId, IData key, IData value, long ttl)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "TransactionalMap.Put";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeGuid(initialFrame.Content, RequestTxnIdFieldOffset, txnId);
            EncodeLong(initialFrame.Content, RequestThreadIdFieldOffset, threadId);
            EncodeLong(initialFrame.Content, RequestTtlFieldOffset, ttl);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            DataCodec.Encode(clientMessage, @value);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.TxnId =  DecodeGuid(initialFrame.Content, RequestTxnIdFieldOffset);
            request.ThreadId =  DecodeLong(initialFrame.Content, RequestThreadIdFieldOffset);
            request.Ttl =  DecodeLong(initialFrame.Content, RequestTtlFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            request.Key = DataCodec.Decode(iterator);
            request.Value = DataCodec.Decode(iterator);
            return request;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// Previous value associated with key or  null if there was no mapping for key
            ///</summary>
            public IData Response;
        }

        public static ClientMessage EncodeResponse(IData response)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            CodecUtil.EncodeNullable(clientMessage, response, DataCodec.Encode);
            return clientMessage;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            response.Response = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
            return response;
        }
    }
}