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

using System.Collections.Generic;
using Hazelcast.Messaging;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Protocol.Data;

namespace Hazelcast.Protocol
{
    // TODO rename, this is not really a 'codec'
    // and, it cannot be in BuiltIn because BuildIn is used by everything, but this one uses Custom
    internal static class ErrorsCodec
    {
        // Other codecs message types can be in range 0x000100 - 0xFFFFFF
        // So, it is safe to supply a custom message type for exceptions in
        // the range 0x000000 - 0x0000FF
        public const int ExceptionMessageType = 0;
        // private const int InitialFrameSize = ResponseBackupAcksFieldOffset + IntSizeInBytes;

        // public static ClientMessage Encode(List<ErrorHolder> errorHolders)
        // {
        //     var clientMessage = CreateForEncode();
        //     var initialFrame = new Frame(new byte[InitialFrameSize], UnfragmentedMessage);
        //     clientMessage.Add(initialFrame);
        //     clientMessage.MessageType = ExceptionMessageType;
        //     ListMultiFrameCodec.Encode(clientMessage, errorHolders, ErrorHolderCodec.Encode);
        //     return clientMessage;
        // }

        public static List<ErrorHolder> Decode(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            //initial frame
            iterator.Take();
            return ListMultiFrameCodec.Decode(iterator, ErrorHolderCodec.Decode);
        }
    }
}