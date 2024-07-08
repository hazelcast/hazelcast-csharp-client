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
    /// The message is used to transfer the declarative pipeline definition and the related resource files from client to the server.
    ///</summary>
#if SERVER_CODEC
    internal static class ExperimentalPipelineSubmitServerCodec
#else
    internal static class ExperimentalPipelineSubmitCodec
#endif
    {
        public const int RequestMessageType = 16580864; // 0xFD0100
        public const int ResponseMessageType = 16580865; // 0xFD0101
        private const int RequestResourceBundleChecksumFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestResourceBundleChecksumFieldOffset + BytesExtensions.SizeOfInt;
        private const int ResponseJobIdFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseJobIdFieldOffset + BytesExtensions.SizeOfLong;

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// The name of the submitted Job using this pipeline.
            ///</summary>
            public string JobName { get; set; }

            /// <summary>
            /// The definition of the pipeline steps. It currently uses the YAML format.
            ///</summary>
            public string PipelineDefinition { get; set; }

            /// <summary>
            /// This is the zipped file which contains the user project folders and files. For Python project, it is the Python project files. It is optional in the sense that if the user likes to use a user docker image with all the resources and project files included, this parameter can be null.
            ///</summary>
            public byte[] ResourceBundle { get; set; }

            /// <summary>
            /// This is the CRC32 checksum over the resource bundle bytes.
            ///</summary>
            public int ResourceBundleChecksum { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(string jobName, string pipelineDefinition, byte[] resourceBundle, int resourceBundleChecksum)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = true,
                OperationName = "Experimental.PipelineSubmit"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteIntL(RequestResourceBundleChecksumFieldOffset, resourceBundleChecksum);
            clientMessage.Append(initialFrame);
            CodecUtil.EncodeNullable(clientMessage, jobName, StringCodec.Encode);
            StringCodec.Encode(clientMessage, pipelineDefinition);
            CodecUtil.EncodeNullable(clientMessage, resourceBundle, ByteArrayCodec.Encode);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.ResourceBundleChecksum = initialFrame.Bytes.ReadIntL(RequestResourceBundleChecksumFieldOffset);
            request.JobName = CodecUtil.DecodeNullable(iterator, StringCodec.Decode);
            request.PipelineDefinition = StringCodec.Decode(iterator);
            request.ResourceBundle = CodecUtil.DecodeNullable(iterator, ByteArrayCodec.Decode);
            return request;
        }
#endif

        public sealed class ResponseParameters
        {

            /// <summary>
            /// This is the unique identifier for the job which is created for this pipeline
            ///</summary>
            public long JobId { get; set; }
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse(long jobId)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            initialFrame.Bytes.WriteLongL(ResponseJobIdFieldOffset, jobId);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.JobId = initialFrame.Bytes.ReadLongL(ResponseJobIdFieldOffset);
            return response;
        }

    }
}