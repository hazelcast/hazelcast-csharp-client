// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Protocol.Models;

namespace Hazelcast.Testing.Protocol
{
    internal static class ErrorsServerCodec
    {
        // Other codecs message types can be in range 0x000100 - 0xFFFFFF
        // So, it is safe to supply a custom message type for exceptions in
        // the range 0x000000 - 0x0000FF
        public const int ExceptionMessageType = 0;
        private const int InitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfInt;

        // or should this be available for all codecs, EncodeResponse, in the testing project?
        public static ClientMessage EncodeResponse(IEnumerable<ErrorHolder> errorHolders)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[InitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            clientMessage.Append(initialFrame);
            clientMessage.MessageType = ExceptionMessageType;
            ListMultiFrameCodec.Encode(clientMessage, errorHolders, ErrorHolderCodec.Encode);
            return clientMessage;
        }
    }
}
