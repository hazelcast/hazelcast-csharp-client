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

//package com.hazelcast.client.impl.protocol.codec.builtin;

//import com.hazelcast.client.impl.protocol.ClientMessage;
//import com.hazelcast.core.HazelcastException;
//import com.hazelcast.nio.Address;
//import com.hazelcast.nio.Bits;

//import java.net.UnknownHostException;
//import java.util.ListIterator;

//import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
//import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
//import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
//import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeInt;
//import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeInt;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class AddressCodec
    {
        private const int PortOffset = 0;
        private const int InitialFrameSize = PortOffset + Bits.IntSizeInBytes;

        public static void Encode(ClientMessage clientMessage, Address address)
        {
            clientMessage.add(BeginFrame);
            ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
            EncodeInt(initialFrame.content, PortOffset, address.getPort());
            clientMessage.add(initialFrame);
            StringCodec.Encode(clientMessage, address.getHost());
            clientMessage.add(EndFrame);
        }

        public static Address Decode(ListIterator<ClientMessage.Frame> iterator)
        {
            // begin frame
            iterator.next();
            ClientMessage.Frame initialFrame = iterator.next();
            int port = DecodeInt(initialFrame.content, PortOffset);
            String host = StringCodec.Decode(iterator);
            fastForwardToEndFrame(iterator);
            try
            {
                return new Address(host, port);
            }
            catch (UnknownHostException e)
            {
                throw new HazelcastException(e);
            }
        }
    }
}