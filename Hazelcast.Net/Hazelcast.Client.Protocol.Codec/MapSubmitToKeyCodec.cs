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
    /// Applies the user defined EntryProcessor to the entry mapped by the key. Returns immediately with a Future
    /// representing that task.EntryProcessor is not cancellable, so calling Future.cancel() method won't cancel the
    /// operation of EntryProcessor.
    ///</summary>
    internal static class MapSubmitToKeyCodec 
    {
        //hex: 0x013300
        public const int RequestMessageType = 78592;
        //hex: 0x013301
        public const int ResponseMessageType = 78593;
        private const int RequestThreadIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestThreadIdFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + IntSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// name of map
            ///</summary>
            public string Name;

            /// <summary>
            /// entry processor to be executed on the entry.
            ///</summary>
            public IData EntryProcessor;

            /// <summary>
            /// the key of the map entry.
            ///</summary>
            public IData Key;

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long ThreadId;
        }

        public static ClientMessage EncodeRequest(string name, IData entryProcessor, IData key, long threadId) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Map.SubmitToKey";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeLong(initialFrame.Content, RequestThreadIdFieldOffset, threadId);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, entryProcessor);
            DataCodec.Encode(clientMessage, key);
            return clientMessage;
        }

        public static MapSubmitToKeyCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.ThreadId =  DecodeLong(initialFrame.Content, RequestThreadIdFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            request.EntryProcessor = DataCodec.Decode(ref iterator);
            request.Key = DataCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// result of entry process.
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

        public static MapSubmitToKeyCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            response.Response = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
            return response;
        }
    }
}