/*
 * Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol.Codec.Custom
{
    /*
    * This file is auto-generated by the Hazelcast Client Protocol Code Generator.
    * To change this file, edit the templates or the protocol
    * definitions on the https://github.com/hazelcast/hazelcast-client-protocol
    * and regenerate it.
    */
    internal static class ErrorHolderCodec 
    {
        private const int ErrorCodeFieldOffset = 0;
        private const int InitialFrameSize = ErrorCodeFieldOffset + IntSizeInBytes;

        public static void Encode(ClientMessage clientMessage, com.hazelcast.client.impl.protocol.exception.ErrorHolder errorHolder) 
        {
            clientMessage.Add(BeginFrame);

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            EncodeInt(initialFrame.Content, ErrorCodeFieldOffset, errorHolder.ErrorCode);
            clientMessage.Add(initialFrame);

            StringCodec.Encode(clientMessage, errorHolder.ClassName);
            CodecUtil.EncodeNullable(clientMessage, errorHolder.Message, StringCodec.Encode);
            ListMultiFrameCodec.Encode(clientMessage, errorHolder.StackTraceElements(), StackTraceElementCodec.Encode);

            clientMessage.Add(EndFrame);
        }

        public static com.hazelcast.client.impl.protocol.exception.ErrorHolder Decode(ref FrameIterator iterator) 
        {
            // begin frame
            iterator.Next();

            ref var initialFrame = ref iterator.Next();
            var errorCode = DecodeInt(initialFrame.Content, ErrorCodeFieldOffset);

            var className = StringCodec.Decode(ref iterator);
            var message = CodecUtil.DecodeNullable(ref iterator, StringCodec.Decode);
            var stackTraceElements = ListMultiFrameCodec.Decode(ref iterator, StackTraceElementCodec.Decode);

            CodecUtil.FastForwardToEndFrame(ref iterator);

            return new com.hazelcast.client.impl.protocol.exception.ErrorHolder(errorCode, className, message, stackTraceElements);
        }
    }
}