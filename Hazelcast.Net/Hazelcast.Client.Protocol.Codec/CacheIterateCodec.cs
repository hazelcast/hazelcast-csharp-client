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
    /// The ordering of iteration over entries is undefined. During iteration, any entries that are a). read will have
    /// their appropriate CacheEntryReadListeners notified and b). removed will have their appropriate
    /// CacheEntryRemoveListeners notified. java.util.Iterator#next() may return null if the entry is no longer present,
    /// has expired or has been evicted.
    ///</summary>
    internal static class CacheIterateCodec 
    {
        public const int RequestMessageType = 0x150F00;
        public const int ResponseMessageType = 0x150F01;
        private const int RequestTableIndexFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestBatchFieldOffset = RequesttableIndexFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestBatchFieldOffset + IntSizeInBytes;
        private const int ResponseTableIndexFieldOffset = CorrelationIdFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseTableIndexFieldOffset + IntSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the cache.
            ///</summary>
            public string Name;

            /// <summary>
            /// The slot number (or index) to start the iterator
            ///</summary>
            public int TableIndex;

            /// <summary>
            /// The number of items to be batched
            ///</summary>
            public int Batch;
        }

        public static ClientMessage EncodeRequest(string name, int tableIndex, int batch) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Cache.Iterate";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame.Content, RequestTableIndexFieldOffset, tableIndex);
            EncodeInt(initialFrame.Content, RequestBatchFieldOffset, batch);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static CacheIterateCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.TableIndex =  DecodeInt(initialFrame.Content, RequestTableIndexFieldOffset);
            request.Batch =  DecodeInt(initialFrame.Content, RequestBatchFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

             /// <summary>
            /// The slot number (or index) to start the iterator
            ///</summary>
            public int TableIndex;

             /// <summary>
            /// TODO DOC
            ///</summary>
            public IEnumerable<IData> Keys;
        }

        public static ClientMessage EncodeResponse(int tableIndex, IEnumerable<IData> keys) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeInt(initialFrame.Content, ResponseTableIndexFieldOffset, tableIndex);
            ListMultiFrameCodec.Encode(clientMessage, keys, DataCodec.Encode);
            return clientMessage;
        }

        public static CacheIterateCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.TableIndex = DecodeInt(initialFrame.Content, ResponseTableIndexFieldOffset);
            response.Keys = ListMultiFrameCodec.Decode(ref iterator, DataCodec.Decode);
            return response;
        }
    }
}