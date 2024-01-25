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
    /// Adds a new executor configuration to a running cluster.
    /// If an executor configuration with the given {@code name} already exists, then
    /// the new configuration is ignored and the existing one is preserved.
    ///</summary>
#if SERVER_CODEC
    internal static class DynamicConfigAddExecutorConfigServerCodec
#else
    internal static class DynamicConfigAddExecutorConfigCodec
#endif
    {
        public const int RequestMessageType = 1771520; // 0x1B0800
        public const int ResponseMessageType = 1771521; // 0x1B0801
        private const int RequestPoolSizeFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestQueueCapacityFieldOffset = RequestPoolSizeFieldOffset + BytesExtensions.SizeOfInt;
        private const int RequestStatisticsEnabledFieldOffset = RequestQueueCapacityFieldOffset + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestStatisticsEnabledFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// executor's name
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// executor thread pool size
            ///</summary>
            public int PoolSize { get; set; }

            /// <summary>
            /// capacity of executor queue. A value of {@code 0} implies {@link Integer#MAX_VALUE}
            ///</summary>
            public int QueueCapacity { get; set; }

            /// <summary>
            /// {@code true} to enable gathering of statistics, otherwise {@code false}
            ///</summary>
            public bool StatisticsEnabled { get; set; }

            /// <summary>
            /// name of an existing configured split brain protection to be used to determine the minimum number of members
            /// required in the cluster for the lock to remain functional. When {@code null}, split brain protection does not
            /// apply to this lock configuration's operations.
            ///</summary>
            public string SplitBrainProtectionName { get; set; }

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

        public static ClientMessage EncodeRequest(string name, int poolSize, int queueCapacity, bool statisticsEnabled, string splitBrainProtectionName, string @namespace)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "DynamicConfig.AddExecutorConfig"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteIntL(RequestPoolSizeFieldOffset, poolSize);
            initialFrame.Bytes.WriteIntL(RequestQueueCapacityFieldOffset, queueCapacity);
            initialFrame.Bytes.WriteBoolL(RequestStatisticsEnabledFieldOffset, statisticsEnabled);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            CodecUtil.EncodeNullable(clientMessage, splitBrainProtectionName, StringCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, @namespace, StringCodec.Encode);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.PoolSize = initialFrame.Bytes.ReadIntL(RequestPoolSizeFieldOffset);
            request.QueueCapacity = initialFrame.Bytes.ReadIntL(RequestQueueCapacityFieldOffset);
            request.StatisticsEnabled = initialFrame.Bytes.ReadBoolL(RequestStatisticsEnabledFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            request.SplitBrainProtectionName = CodecUtil.DecodeNullable(iterator, StringCodec.Decode);
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
