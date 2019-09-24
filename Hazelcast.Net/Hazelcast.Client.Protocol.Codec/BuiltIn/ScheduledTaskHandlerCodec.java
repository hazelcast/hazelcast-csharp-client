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
import com.hazelcast.nio.Address;
import com.hazelcast.nio.Bits;
import com.hazelcast.scheduledexecutor.ScheduledTaskHandler;
import com.hazelcast.scheduledexecutor.impl.ScheduledTaskHandlerImpl;

import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.DecodeNullable;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.EncodeNullable;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeInt;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeInt;

public final class ScheduledTaskHandlerCodec {
    private const int PARTITION_ID_OFFSET = 0;
    private const int InitialFrameSize = PARTITION_ID_OFFSET + Bits.IntSizeInBytes;

    private ScheduledTaskHandlerCodec() {
    }

    public static void Encode(ClientMessage clientMessage, ScheduledTaskHandler handler) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeInt(initialFrame.content, PARTITION_ID_OFFSET, handler.getPartitionId());
        clientMessage.add(initialFrame);

        StringCodec.Encode(clientMessage, handler.getSchedulerName());
        StringCodec.Encode(clientMessage, handler.getTaskName());
        EncodeNullable(clientMessage, handler.getAddress(), AddressCodec::Encode);

        clientMessage.add(EndFrame);
    }

    public static ScheduledTaskHandler Decode(ListIterator<ClientMessage.Frame> iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        int partitionId = DecodeInt(initialFrame.content, PARTITION_ID_OFFSET);

        String schedulerName = StringCodec.Decode(iterator);
        String taskName = StringCodec.Decode(iterator);
        Address address = DecodeNullable(iterator, AddressCodec::Decode);

        fastForwardToEndFrame(iterator);

        if (address == null) {
            return ScheduledTaskHandlerImpl.of(partitionId, schedulerName, taskName);
        } else {
            return ScheduledTaskHandlerImpl.of(address, schedulerName, taskName);
        }
    }
}
