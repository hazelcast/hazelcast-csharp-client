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
import com.hazelcast.transaction.impl.xa.SerializableXID;

import javax.transaction.xa.Xid;
import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.IntSizeInBytes;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeInt;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeInt;

public final class XidCodec {
    private const int FORMAT_ID_OFFSET = 0;
    private const int InitialFrameSize = FORMAT_ID_OFFSET + IntSizeInBytes;

    private XidCodec() {
    }

    public static void Encode(ClientMessage clientMessage, Xid xid) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeInt(initialFrame.content, FORMAT_ID_OFFSET, xid.getFormatId());
        clientMessage.add(initialFrame);

        ByteArrayCodec.Encode(clientMessage, xid.getGlobalTransactionId());
        ByteArrayCodec.Encode(clientMessage, xid.getBranchQualifier());

        clientMessage.add(EndFrame);
    }

    public static Xid Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        int formatId = DecodeInt(initialFrame.content, FORMAT_ID_OFFSET);

        byte[] globalTransactionId = ByteArrayCodec.Decode(iterator);
        byte[] branchQualifier = ByteArrayCodec.Decode(iterator);

        fastForwardToEndFrame(iterator);

        return new SerializableXID(formatId, globalTransactionId, branchQualifier);
    }
}
