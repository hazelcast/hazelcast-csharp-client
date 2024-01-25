﻿// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
//   Hazelcast Client Protocol Code Generator @0a5719d
//   https://github.com/hazelcast/hazelcast-client-protocol
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
    /// Adds a new topic configuration to a running cluster.
    /// If a topic configuration with the given {@code name} already exists, then
    /// the new configuration is ignored and the existing one is preserved.
    ///</summary>
#if SERVER_CODEC
    internal static class DynamicConfigAddTopicConfigServerCodec
#else
    internal static class DynamicConfigAddTopicConfigCodec
#endif
    {
        public const int RequestMessageType = 1771264; // 0x1B0700
        public const int ResponseMessageType = 1771265; // 0x1B0701
        private const int RequestGlobalOrderingEnabledFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestStatisticsEnabledFieldOffset = RequestGlobalOrderingEnabledFieldOffset + BytesExtensions.SizeOfBool;
        private const int RequestMultiThreadingEnabledFieldOffset = RequestStatisticsEnabledFieldOffset + BytesExtensions.SizeOfBool;
        private const int RequestInitialFrameSize = RequestMultiThreadingEnabledFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// topic's name
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// when {@code true} all nodes listening to the same topic get their messages in
            /// the same order
            ///</summary>
            public bool GlobalOrderingEnabled { get; set; }

            /// <summary>
            /// {@code true} to enable gathering of statistics, otherwise {@code false}
            ///</summary>
            public bool StatisticsEnabled { get; set; }

            /// <summary>
            /// {@code true} to enable multi-threaded processing of incoming messages, otherwise
            /// a single thread will handle all topic messages
            ///</summary>
            public bool MultiThreadingEnabled { get; set; }

            /// <summary>
            /// message listener configurations
            ///</summary>
            public ICollection<Hazelcast.Protocol.Models.ListenerConfigHolder> ListenerConfigs { get; set; }

            /// <summary>
            /// Name of the namespace applied to this instance.
            ///</summary>
            public string Namespace { get; set; }

            /// <summary>
            /// <c>true</c> if the namespace is received from the client, <c>false</c> otherwise.
            /// If this is false, namespace has the default value for its type.
            /// </summary>
            public bool IsNamespaceExists { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(string name, bool globalOrderingEnabled, bool statisticsEnabled, bool multiThreadingEnabled, ICollection<Hazelcast.Protocol.Models.ListenerConfigHolder> listenerConfigs, string @namespace)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "DynamicConfig.AddTopicConfig"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteBoolL(RequestGlobalOrderingEnabledFieldOffset, globalOrderingEnabled);
            initialFrame.Bytes.WriteBoolL(RequestStatisticsEnabledFieldOffset, statisticsEnabled);
            initialFrame.Bytes.WriteBoolL(RequestMultiThreadingEnabledFieldOffset, multiThreadingEnabled);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            ListMultiFrameCodec.EncodeNullable(clientMessage, listenerConfigs, ListenerConfigHolderCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, @namespace, StringCodec.Encode);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.GlobalOrderingEnabled = initialFrame.Bytes.ReadBoolL(RequestGlobalOrderingEnabledFieldOffset);
            request.StatisticsEnabled = initialFrame.Bytes.ReadBoolL(RequestStatisticsEnabledFieldOffset);
            request.MultiThreadingEnabled = initialFrame.Bytes.ReadBoolL(RequestMultiThreadingEnabledFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            request.ListenerConfigs = ListMultiFrameCodec.DecodeNullable(iterator, ListenerConfigHolderCodec.Decode);
            if (iterator.Current?.Next != null)
            {
                request.Namespace = CodecUtil.DecodeNullable(iterator, StringCodec.Decode);
                request.IsNamespaceExists = true;
            }
            else request.IsNamespaceExists = false;
            return request;
        }
#endif

        public sealed class ResponseParameters
        {
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse()
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            iterator.Take(); // empty initial frame
            return response;
        }

    }
}
