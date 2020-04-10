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
    /// Proxies will be created on all cluster members.
    /// If the member is  a lite member, a replicated map will not be created.
    /// Any proxy creation failure is logged on the server side.
    /// Exceptions related to a proxy creation failure is not send to the client.
    /// A proxy creation failure does not cancel this operation, all proxies will be attempted to be created.
    ///</summary>
    internal static class ClientCreateProxiesCodec
    {
        public const int RequestMessageType = 3584; // 0x000E00
        public const int ResponseMessageType = 3585; // 0x000E01
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + IntSizeInBytes;

        public static ClientMessage EncodeRequest(ICollection<KeyValuePair<string, string>> proxies)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Client.CreateProxies";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            EntryListCodec.Encode(clientMessage, proxies, StringCodec.Encode, StringCodec.Encode);
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