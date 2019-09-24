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

import java.util.List;
import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.CORRELATION_ID_FIELD_OFFSET;
import static com.hazelcast.client.impl.protocol.ClientMessage.UNFRAGMENTED_MESSAGE;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.LONG_SIZE_IN_BYTES;

public final class ErrorsCodec {

    private const int InitialFrameSize = CORRELATION_ID_FIELD_OFFSET + LONG_SIZE_IN_BYTES;

    private ErrorsCodec() {
    }

    public static ClientMessage Encode(List<ErrorHolder> errorHolders) {
        ClientMessage clientMessage = ClientMessage.createForEncode();
        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize], UNFRAGMENTED_MESSAGE);
        clientMessage.add(initialFrame);
        clientMessage.setMessageType(ErrorCodec.EXCEPTION_MESSAGE_TYPE);
        ListMultiFrameCodec.Encode(clientMessage, errorHolders, ErrorCodec::Encode);
        return clientMessage;
    }

    public static List<ErrorHolder> Decode(ClientMessage clientMessage) {
        ListIterator<ClientMessage.Frame> iterator = clientMessage.listIterator();
        //initial frame
        iterator.next();
        return ListMultiFrameCodec.Decode(iterator, ErrorCodec::Decode);
    }
}
