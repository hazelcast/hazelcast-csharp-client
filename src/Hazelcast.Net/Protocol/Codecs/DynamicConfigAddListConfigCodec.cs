﻿// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
//   Hazelcast Client Protocol Code Generator @c89bc95
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
    /// Adds a new list configuration to a running cluster.
    /// If a list configuration with the given {@code name} already exists, then
    /// the new configuration is ignored and the existing one is preserved.
    ///</summary>
#if SERVER_CODEC
    internal static class DynamicConfigAddListConfigServerCodec
#else
    internal static class DynamicConfigAddListConfigCodec
#endif
    {
        public const int RequestMessageType = 1770496; // 0x1B0400
        public const int ResponseMessageType = 1770497; // 0x1B0401
        private const int RequestBackupCountFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestAsyncBackupCountFieldOffset = RequestBackupCountFieldOffset + BytesExtensions.SizeOfInt;
        private const int RequestMaxSizeFieldOffset = RequestAsyncBackupCountFieldOffset + BytesExtensions.SizeOfInt;
        private const int RequestStatisticsEnabledFieldOffset = RequestMaxSizeFieldOffset + BytesExtensions.SizeOfInt;
        private const int RequestMergeBatchSizeFieldOffset = RequestStatisticsEnabledFieldOffset + BytesExtensions.SizeOfBool;
        private const int RequestInitialFrameSize = RequestMergeBatchSizeFieldOffset + BytesExtensions.SizeOfInt;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// list's name
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// item listener configurations
            ///</summary>
            public ICollection<Hazelcast.Protocol.Models.ListenerConfigHolder> ListenerConfigs { get; set; }

            /// <summary>
            /// number of synchronous backups
            ///</summary>
            public int BackupCount { get; set; }

            /// <summary>
            /// number of asynchronous backups
            ///</summary>
            public int AsyncBackupCount { get; set; }

            /// <summary>
            /// maximum size of the list
            ///</summary>
            public int MaxSize { get; set; }

            /// <summary>
            /// {@code true} to enable gathering of statistics on the list, otherwise {@code false}
            ///</summary>
            public bool StatisticsEnabled { get; set; }

            /// <summary>
            /// name of an existing configured split brain protection to be used to determine the minimum number of members
            /// required in the cluster for the lock to remain functional. When {@code null}, split brain protection does not
            /// apply to this lock configuration's operations.
            ///</summary>
            public string SplitBrainProtectionName { get; set; }

            /// <summary>
            /// Name of a class implementing SplitBrainMergePolicy that handles merging of values for this cache
            /// while recovering from network partitioning.
            ///</summary>
            public string MergePolicy { get; set; }

            /// <summary>
            /// Number of entries to be sent in a merge operation.
            ///</summary>
            public int MergeBatchSize { get; set; }

            /// <summary>
            /// Name of the User Code Namespace applied to this instance.
            ///</summary>
            public string UserCodeNamespace { get; set; }

            /// <summary>
            /// <c>true</c> if the userCodeNamespace is received from the client, <c>false</c> otherwise.
            /// If this is false, userCodeNamespace has the default value for its type.
            /// </summary>
            public bool IsUserCodeNamespaceExists { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(string name, ICollection<Hazelcast.Protocol.Models.ListenerConfigHolder> listenerConfigs, int backupCount, int asyncBackupCount, int maxSize, bool statisticsEnabled, string splitBrainProtectionName, string mergePolicy, int mergeBatchSize, string userCodeNamespace)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "DynamicConfig.AddListConfig"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteIntL(RequestBackupCountFieldOffset, backupCount);
            initialFrame.Bytes.WriteIntL(RequestAsyncBackupCountFieldOffset, asyncBackupCount);
            initialFrame.Bytes.WriteIntL(RequestMaxSizeFieldOffset, maxSize);
            initialFrame.Bytes.WriteBoolL(RequestStatisticsEnabledFieldOffset, statisticsEnabled);
            initialFrame.Bytes.WriteIntL(RequestMergeBatchSizeFieldOffset, mergeBatchSize);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            ListMultiFrameCodec.EncodeNullable(clientMessage, listenerConfigs, ListenerConfigHolderCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, splitBrainProtectionName, StringCodec.Encode);
            StringCodec.Encode(clientMessage, mergePolicy);
            CodecUtil.EncodeNullable(clientMessage, userCodeNamespace, StringCodec.Encode);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.BackupCount = initialFrame.Bytes.ReadIntL(RequestBackupCountFieldOffset);
            request.AsyncBackupCount = initialFrame.Bytes.ReadIntL(RequestAsyncBackupCountFieldOffset);
            request.MaxSize = initialFrame.Bytes.ReadIntL(RequestMaxSizeFieldOffset);
            request.StatisticsEnabled = initialFrame.Bytes.ReadBoolL(RequestStatisticsEnabledFieldOffset);
            request.MergeBatchSize = initialFrame.Bytes.ReadIntL(RequestMergeBatchSizeFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            request.ListenerConfigs = ListMultiFrameCodec.DecodeNullable(iterator, ListenerConfigHolderCodec.Decode);
            request.SplitBrainProtectionName = CodecUtil.DecodeNullable(iterator, StringCodec.Decode);
            request.MergePolicy = StringCodec.Decode(iterator);
            if (iterator.Current?.Next != null)
            {
                request.UserCodeNamespace = CodecUtil.DecodeNullable(iterator, StringCodec.Decode);
                request.IsUserCodeNamespaceExists = true;
            }
            else request.IsUserCodeNamespaceExists = false;
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
