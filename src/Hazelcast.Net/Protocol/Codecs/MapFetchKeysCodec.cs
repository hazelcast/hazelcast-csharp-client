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
using System.Threading;
using System.Threading.Tasks;
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
    /// Fetches specified number of keys from the specified partition starting from specified table index.
    ///</summary>
    internal static class MapFetchKeysCodec
    {
        public const int RequestMessageType = 79616; // 0x013700
        public const int ResponseMessageType = 79617; // 0x013701
        private const int RequestBatchFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestBatchFieldOffset + BytesExtensions.SizeOfInt;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

        public static ClientMessage EncodeRequest(string name, ICollection<KeyValuePair<int, int>> iterationPointers, int batch)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = true,
                OperationName = "Map.FetchKeys"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteIntL(RequestBatchFieldOffset, batch);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            EntryListIntegerIntegerCodec.Encode(clientMessage, iterationPointers);
            return clientMessage;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// The index-size pairs that define the state of iteration
            ///</summary>
            public IList<KeyValuePair<int, int>> IterationPointers { get; set; }

            /// <summary>
            /// List of keys.
            ///</summary>
            public IList<IData> Keys { get; set; }
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            iterator.Take(); // empty initial frame
            response.IterationPointers = EntryListIntegerIntegerCodec.Decode(iterator);
            response.Keys = ListMultiFrameCodec.Decode(iterator, DataCodec.Decode);
            return response;
        }

    }
}