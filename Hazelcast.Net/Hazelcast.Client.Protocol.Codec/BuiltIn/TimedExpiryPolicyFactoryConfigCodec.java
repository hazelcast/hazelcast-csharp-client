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
import com.hazelcast.config.CacheSimpleConfig.ExpiryPolicyFactoryConfig.DurationConfig;
import com.hazelcast.config.CacheSimpleConfig.ExpiryPolicyFactoryConfig.TimedExpiryPolicyFactoryConfig;
import com.hazelcast.nio.Bits;

import java.util.ListIterator;
import java.util.concurrent.TimeUnit;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeLong;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeLong;

public final class TimedExpiryPolicyFactoryConfigCodec {
    private const int DURATION_AMOUNT_OFFSET = 0;
    private const int InitialFrameSize = DURATION_AMOUNT_OFFSET + Bits.LONG_SIZE_IN_BYTES;

    private TimedExpiryPolicyFactoryConfigCodec() {
    }

    public static void Encode(ClientMessage clientMessage, TimedExpiryPolicyFactoryConfig config) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeLong(initialFrame.content, DURATION_AMOUNT_OFFSET, config.getDurationConfig().getDurationAmount());
        clientMessage.add(initialFrame);

        StringCodec.Encode(clientMessage, config.getExpiryPolicyType().name());
        StringCodec.Encode(clientMessage, config.getDurationConfig().getTimeUnit().name());

        clientMessage.add(EndFrame);
    }

    public static TimedExpiryPolicyFactoryConfig Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        long durationAmount = DecodeLong(initialFrame.content, DURATION_AMOUNT_OFFSET);

        String expiryPolicyTypeName = StringCodec.Decode(iterator);
        String durationTimeUnitName = StringCodec.Decode(iterator);

        fastForwardToEndFrame(iterator);

        return new TimedExpiryPolicyFactoryConfig(
                TimedExpiryPolicyFactoryConfig.ExpiryPolicyType.valueOf(expiryPolicyTypeName),
                new DurationConfig(durationAmount, TimeUnit.valueOf(durationTimeUnitName)));
    }
}
