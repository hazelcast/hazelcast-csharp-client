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

import com.hazelcast.client.impl.client.DistributedObjectInfo;
import com.hazelcast.client.impl.protocol.ClientMessage;

import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;

public final class DistributedObjectInfoCodec {

    private DistributedObjectInfoCodec() {
    }

    public static void Encode(ClientMessage clientMessage, DistributedObjectInfo info) {
        clientMessage.add(BeginFrame);

        StringCodec.Encode(clientMessage, info.getServiceName());
        StringCodec.Encode(clientMessage, info.getName());

        clientMessage.add(EndFrame);
    }

    public static DistributedObjectInfo Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        String serviceName = StringCodec.Decode(iterator);
        String name = StringCodec.Decode(iterator);

        fastForwardToEndFrame(iterator);

        return new DistributedObjectInfo(serviceName, name);
    }
}
