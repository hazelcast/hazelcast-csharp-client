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
import com.hazelcast.nio.Bits;

import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;

public final class StackTraceElementCodec {

    private const int LINE_NUMBER = 0;
    private const int InitialFrameSize = LINE_NUMBER + Bits.IntSizeInBytes;

    private StackTraceElementCodec() {
    }

    public static void Encode(ClientMessage clientMessage, StackTraceElement stackTraceElement) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        FixedSizeTypesCodec.EncodeInt(initialFrame.content, LINE_NUMBER, stackTraceElement.getLineNumber());
        clientMessage.add(initialFrame);

        StringCodec.Encode(clientMessage, stackTraceElement.getClassName());
        StringCodec.Encode(clientMessage, stackTraceElement.getMethodName());
        CodecUtil.EncodeNullable(clientMessage, stackTraceElement.getFileName(), StringCodec::Encode);

        clientMessage.add(EndFrame);
    }

    public static StackTraceElement Decode(ListIterator<ClientMessage.Frame> iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        int lineNumber = FixedSizeTypesCodec.DecodeInt(initialFrame.content, LINE_NUMBER);

        String declaringClass = StringCodec.Decode(iterator);
        String methodName = StringCodec.Decode(iterator);
        String fileName = CodecUtil.DecodeNullable(iterator, StringCodec::Decode);

        fastForwardToEndFrame(iterator);
        return new StackTraceElement(declaringClass, methodName, fileName, lineNumber);
    }
}
