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
    /// Creates a cluster-wide proxy with the given name and service.
    ///</summary>
    internal static class ClientCreateProxyServerCodec
    {
        public const int RequestMessageType = 1024; // 0x000400
        public const int ResponseMessageType = 1025; // 0x000401
        private const int RequestInitialFrameSize = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

        public sealed class RequestParameters
        {

            /// <summary>
            /// The distributed object name for which the proxy is being created for.
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// The name of the service. Possible service names are:
            /// "hz:impl:listService"
            /// "hz:impl:queueService"
            /// "hz:impl:setService"
            /// "hz:impl:idGeneratorService"
            /// "hz:impl:executorService"
            /// "hz:impl:mapService"
            /// "hz:impl:multiMapService"
            /// "hz:impl:splitBrainProtectionService"
            /// "hz:impl:replicatedMapService"
            /// "hz:impl:ringbufferService"
            /// "hz:core:proxyService"
            /// "hz:impl:reliableTopicService"
            /// "hz:impl:topicService"
            /// "hz:core:txManagerService"
            /// "hz:impl:xaService"
            ///</summary>
            public string ServiceName { get; set; }
        }
    
        public static ClientMessage EncodeRequest(string name, string serviceName)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "Client.CreateProxy"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            StringCodec.Encode(clientMessage, serviceName);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            iterator.Take(); // empty initial frame
            request.Name = StringCodec.Decode(iterator);
            request.ServiceName = StringCodec.Decode(iterator);
            return request;
        }
        
        public sealed class ResponseParameters
        {
        }

        public static ClientMessage EncodeResponse()
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }
    
        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            
            iterator.Take(); // empty initial frame
            return response;
        }

    
    }
}