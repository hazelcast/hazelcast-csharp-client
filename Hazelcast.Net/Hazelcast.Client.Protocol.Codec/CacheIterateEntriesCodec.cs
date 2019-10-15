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
    /// Fetches specified number of entries from the specified partition starting from specified table index.
    ///</summary>
    internal static class CacheIterateEntriesCodec 
    {
        //hex: 0x151D00
        public const int RequestMessageType = 1383680;
        //hex: 0x151D01
        public const int ResponseMessageType = 1383681;
        private const int RequestTableIndexFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestBatchFieldOffset = RequestTableIndexFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestBatchFieldOffset + IntSizeInBytes;
        private const int ResponseTableIndexFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
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
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Cache.IterateEntries";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame.Content, RequestTableIndexFieldOffset, tableIndex);
            EncodeInt(initialFrame.Content, RequestBatchFieldOffset, batch);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static CacheIterateEntriesCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
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
            public IEnumerable<KeyValuePair<IData, IData>> Entries;
        }

        public static ClientMessage EncodeResponse(int tableIndex, IEnumerable<KeyValuePair<IData, IData>> entries) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeInt(initialFrame.Content, ResponseTableIndexFieldOffset, tableIndex);
            EntryListDataDataCodec.Encode(clientMessage, entries);
            return clientMessage;
        }

        public static CacheIterateEntriesCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.TableIndex = DecodeInt(initialFrame.Content, ResponseTableIndexFieldOffset);
            response.Entries = EntryListDataDataCodec.Decode(ref iterator);
            return response;
        }
    }
}