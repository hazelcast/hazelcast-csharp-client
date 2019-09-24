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

package com.hazelcast.client.impl.protocol.codec.builtin;

import com.hazelcast.client.impl.protocol.ClientMessage;
import com.hazelcast.client.impl.protocol.exception.ErrorHolder;
import com.hazelcast.nio.Bits;

import java.util.List;
import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;

public final class ErrorCodec {

    // Other codecs message types can be in range 0x000100 - 0xFFFFFF
    // So, it is safe to supply a custom message type for exceptions in
    // the range 0x000000 - 0x0000FF
    public const int EXCEPTION_MESSAGE_TYPE = 0;
    private const int ERROR_CODE = 0;
    private const int InitialFrameSize = ERROR_CODE + Bits.IntSizeInBytes;

    private ErrorCodec() {
    }

    public static void Encode(ClientMessage clientMessage, ErrorHolder errorHolder) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        FixedSizeTypesCodec.EncodeInt(initialFrame.content, ERROR_CODE, errorHolder.getErrorCode());
        clientMessage.add(initialFrame);

        StringCodec.Encode(clientMessage, errorHolder.getClassName());
        CodecUtil.EncodeNullable(clientMessage, errorHolder.getMessage(), StringCodec::Encode);
        ListMultiFrameCodec.Encode(clientMessage, errorHolder.getStackTraceElements(), StackTraceElementCodec::Encode);

        clientMessage.add(EndFrame);
    }

    public static ErrorHolder Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        int errorCode = FixedSizeTypesCodec.DecodeInt(initialFrame.content, ERROR_CODE);

        String className = StringCodec.Decode(iterator);
        String message = CodecUtil.DecodeNullable(iterator, StringCodec::Decode);
        List<StackTraceElement> stackTraceElements = ListMultiFrameCodec.Decode(iterator, StackTraceElementCodec::Decode);

        fastForwardToEndFrame(iterator);
        return new ErrorHolder(errorCode, className, message, stackTraceElements);
    }
}
