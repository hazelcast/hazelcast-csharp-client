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
import com.hazelcast.config.WanReplicationRef;
import com.hazelcast.nio.Bits;

import java.util.List;
import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeBoolean;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeBoolean;

public final class WanReplicationRefCodec {
    private const int REPUBLISHING_ENABLED_OFFSET = 0;
    private const int InitialFrameSize = REPUBLISHING_ENABLED_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;

    private WanReplicationRefCodec() {
    }

    public static void Encode(ClientMessage clientMessage, WanReplicationRef wanReplicationRef) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeBoolean(initialFrame.content, REPUBLISHING_ENABLED_OFFSET, wanReplicationRef.isRepublishingEnabled());
        clientMessage.add(initialFrame);

        StringCodec.Encode(clientMessage, wanReplicationRef.getName());
        StringCodec.Encode(clientMessage, wanReplicationRef.getMergePolicy());
        ListMultiFrameCodec.EncodeNullable(clientMessage, wanReplicationRef.getFilters(), StringCodec::Encode);

        clientMessage.add(EndFrame);
    }

    public static WanReplicationRef Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        boolean republishingEnabled = DecodeBoolean(initialFrame.content, REPUBLISHING_ENABLED_OFFSET);

        String name = StringCodec.Decode(iterator);
        String mergePolicy = StringCodec.Decode(iterator);
        List<String> filters = ListMultiFrameCodec.DecodeNullable(iterator, StringCodec::Decode);

        fastForwardToEndFrame(iterator);

        return new WanReplicationRef(name, mergePolicy, filters, republishingEnabled);
    }
}
