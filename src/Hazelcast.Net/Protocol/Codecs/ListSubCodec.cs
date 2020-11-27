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
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Returns a view of the portion of this list between the specified from, inclusive, and to, exclusive.(If from and
    /// to are equal, the returned list is empty.) The returned list is backed by this list, so non-structural changes in
    /// the returned list are reflected in this list, and vice-versa. The returned list supports all of the optional list
    /// operations supported by this list.
    /// This method eliminates the need for explicit range operations (of the sort that commonly exist for arrays).
    /// Any operation that expects a list can be used as a range operation by passing a subList view instead of a whole list.
    /// Similar idioms may be constructed for indexOf and lastIndexOf, and all of the algorithms in the Collections class
    /// can be applied to a subList.
    /// The semantics of the list returned by this method become undefined if the backing list (i.e., this list) is
    /// structurally modified in any way other than via the returned list.(Structural modifications are those that change
    /// the size of this list, or otherwise perturb it in such a fashion that iterations in progress may yield incorrect results.)
    ///</summary>
#if SERVER_CODEC
    internal static class ListSubServerCodec
#else
    internal static class ListSubCodec
#endif
    {
        public const int RequestMessageType = 333056; // 0x051500
        public const int ResponseMessageType = 333057; // 0x051501
        private const int RequestFromFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestToFieldOffset = RequestFromFieldOffset + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestToFieldOffset + BytesExtensions.SizeOfInt;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// Name of the List
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// Low endpoint (inclusive) of the subList
            ///</summary>
            public int From { get; set; }

            /// <summary>
            /// High endpoint (exclusive) of the subList
            ///</summary>
            public int To { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(string name, int @from, int to)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = true,
                OperationName = "List.Sub"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteIntL(RequestFromFieldOffset, @from);
            initialFrame.Bytes.WriteIntL(RequestToFieldOffset, to);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.From = initialFrame.Bytes.ReadIntL(RequestFromFieldOffset);
            request.To = initialFrame.Bytes.ReadIntL(RequestToFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            return request;
        }
#endif

        public sealed class ResponseParameters
        {

            /// <summary>
            /// A view of the specified range within this list
            ///</summary>
            public IList<IData> Response { get; set; }
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse(ICollection<IData> response)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            clientMessage.Append(initialFrame);
            ListMultiFrameCodec.Encode(clientMessage, response, DataCodec.Encode);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            iterator.Take(); // empty initial frame
            response.Response = ListMultiFrameCodec.Decode(iterator, DataCodec.Decode);
            return response;
        }

    }
}
